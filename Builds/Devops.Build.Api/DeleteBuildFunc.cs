using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
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
    public class DeleteBuildFunc
    {
        private readonly IBuildService _buildService;
        public DeleteBuildFunc(IBuildService buildService)
        {
            _buildService = buildService;
        }

        [FunctionName("DeleteBuild")]
        [OpenApiOperation("DeleteDefinition", "DeleteBuild")]
        [OpenApiParameter("projectName", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter("buildDefinitionId", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(BuildDefinitionDeleteDto))]
        public async Task<IActionResult> DeleteBuildDefinition([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "builds/{projectName}/{buildDefinitionId}")] HttpRequest request, string projectName, string buildDefinitionId,ILogger log)
        {
            log.LogInformation($"DevOpsBuild: Delete DevOps Build request received.");
            var buildDeleteDefinition = new BuildDefinitionDeleteDto();
            try
            {
                buildDeleteDefinition = await _buildService.DeleteBuildDefinition(buildDefinitionId, projectName);
                if (buildDeleteDefinition.Error == null)
                    return new OkObjectResult(JsonConvert.SerializeObject(buildDeleteDefinition));
                else if (buildDeleteDefinition.Error.Status == "BadRequest")
                    return new BadRequestObjectResult(JsonConvert.SerializeObject(buildDeleteDefinition));
                else
                    return new NotFoundObjectResult(JsonConvert.SerializeObject(buildDeleteDefinition));

            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Delete: The http worker received an unexpected error while attempting to delete a build definition. {ex.Message}");
                return new OkObjectResult(JsonConvert.SerializeObject(buildDeleteDefinition));
            }
        }
    }
}
