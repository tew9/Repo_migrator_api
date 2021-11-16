using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using DevOps.Build.Api.Shared.Models;
using DevOps.Build.Api.Shared.Services;
using DevOps.Build.Contracts;
using Microsoft.Azure.Documents;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using System.Net;
using Microsoft.OpenApi.Models;

namespace DevOps.Build.Api.Shared
{
    public class GetAllBuildFunc
    {
        private readonly IBuildService _buildService;
       
        public GetAllBuildFunc(IBuildService buildService)
        {
            _buildService = buildService;
        }

        [FunctionName("GetAllBuild")]
        [OpenApiOperation("GetBuildDefinitions", "GetAllBuildDefinitions")]
        [OpenApiParameter("projectName", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(BuildDefinitionList))]

        public async Task<IActionResult> GetAllBuildDefinition([HttpTrigger(AuthorizationLevel.Function, "get", Route = "builds/{projectName}")] HttpRequest request, string projectName,ILogger log)
        {
            log.LogInformation($"DevOpsBuild: Create DevOps Build request received.");
            var buildDefinitions = new BuildDefinitionList();
            try
            {
                buildDefinitions = await _buildService.GetAllBuildDefinition(projectName);
                if(buildDefinitions.Error == null)
                    return new OkObjectResult(JsonConvert.SerializeObject(buildDefinitions));
               else if(buildDefinitions.Error.Status == "BadRequest")
                    return new BadRequestObjectResult(JsonConvert.SerializeObject(buildDefinitions));
                else
                    return new NotFoundObjectResult(JsonConvert.SerializeObject(buildDefinitions));

            }
            catch (Exception ex)
            {
                log.LogError(ex, $"DevOps GetAllBuilds: The http worker received an unexpected error while attempting to create a build.{ex.Message}");
                return new OkObjectResult(JsonConvert.SerializeObject(buildDefinitions));
            }

        }
    }
}
