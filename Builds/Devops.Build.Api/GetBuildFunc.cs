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
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;

namespace DevOps.Build.Api
{
    public class GetBuildFunc
    {

        private readonly IBuildService _buildService;
 
        public GetBuildFunc(IBuildService buildService)
        {
            _buildService = buildService;
        }

        [FunctionName("GetBuild")]
        [OpenApiOperation("GetBuildDefinition", "GetBuildDefinition")]
        [OpenApiParameter("projectName", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter("buildDefinitionId", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(BuildDefinitionDto))]
        public async Task<IActionResult> GetBuildDefinition([HttpTrigger(AuthorizationLevel.Function, "get", Route = "builds/{projectName}/{buildDefinitionId}")] HttpRequest request,string projectName, string buildDefinitionId,ILogger log)
        {
            log.LogInformation($"DevOpsBuild: Create DevOps Build request received.");
            var buildDefinition = new BuildDefinitionDto();
            try
            {
                buildDefinition = await _buildService.GetBuildDefinition(buildDefinitionId, projectName);
                if (buildDefinition.Error == null)
                    return new OkObjectResult(JsonConvert.SerializeObject(buildDefinition));
                else if (buildDefinition.Error.Status == "BadRequest")
                    return new BadRequestObjectResult(JsonConvert.SerializeObject(buildDefinition));
                else
                    return new NotFoundObjectResult(JsonConvert.SerializeObject(buildDefinition));
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"GetBuildDefinition: The http worker received an unexpected error while attempting to create a pickle.{ex.Message}");
            }
            if (buildDefinition.Error != null)
            {
                return new BadRequestObjectResult(JsonConvert.SerializeObject(buildDefinition));
            }
            return new OkObjectResult(JsonConvert.SerializeObject(buildDefinition));
        }
    }
}
