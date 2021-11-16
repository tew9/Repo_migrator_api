using System;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using DevOps.Build.Api.Shared.Models;
using DevOps.Build.Contracts;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using Newtonsoft.Json;
using DevOps.Build.Api.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using System.Net;

namespace DevOps.Build.Api
{
    public class CreateBuildFunc
    {

        private readonly IBuildService _buildService;
   
        public CreateBuildFunc(IBuildService buildService)
        {
            _buildService = buildService;
        }

        [FunctionName("CreateBuild")]
        [OpenApiOperation("CreateBuildDefinition", "CreateBuildDefinition")]
        [OpenApiRequestBody("application/json", typeof(BuildRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, contentType: "text/plain", typeof(BuildDefinitionDto))]

        public async Task<IActionResult> CreateBuildDefinition([HttpTrigger(AuthorizationLevel.Function, "post", Route = "builds")] HttpRequest request,ILogger log)
        {
            log.LogInformation($"DevOpsBuild: Create DevOps Build request received.");
            var buildDefinition = new BuildDefinitionDto();
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            var requestJson = JsonConvert.DeserializeObject<BuildRequest>(requestBody);
            try
            {
                buildDefinition = await _buildService.CreateBuildDefinition(requestJson.RepoId, requestJson.BuildName, requestJson.ProjectName, requestJson.BuildAgentName, requestJson.TemplateBuildName);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Create: The http worker received an unexpected error while attempting to create a build definition. {requestBody}. {ex.Message}");
            }
            if(buildDefinition.Error != null)
            {
                return new BadRequestObjectResult(JsonConvert.SerializeObject(buildDefinition));
            }
            return new OkObjectResult(JsonConvert.SerializeObject(buildDefinition));
        }
    }
}
