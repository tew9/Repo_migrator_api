using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using DevOps.TaskMaster.Orchestrator.Shared.Models;

namespace DevOps.TaskMaster.Orchestrator
{
    public partial class Orchestrator
    {
        [FunctionName("GetOperation")]
        [OpenApiOperation(operationId: "getRepo", tags: new[] { "getrepo" }, Summary = "get repo", Description = "This Api will get repo with the build and the release associated with it", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "projectName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "The project name", Description = "provide the project name eg: CHK", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "repoName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "The repo name", Description = "provide the repository name eg: tangodemo2", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(GetResponse), Summary = "The response", Description = "This returns the response")]

        public async Task<IActionResult> GetOperation(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "repos/{projectName}/{repoName}")] HttpRequest request, string projectName, string repoName,
        ILogger log)
        {
            GetResponse response = new GetResponse();
            #region GetRepo
            var repoResponse = await _RepoService.GetRepository(projectName, repoName);
            if (repoResponse.IsSuccessStatusCode)
            {
                var responseContent = await repoResponse.Content.ReadAsStringAsync();
                var getRepoStringResult = JsonConvert.DeserializeObject(responseContent).ToString();
                var repoResult = JsonConvert.DeserializeObject<Repository>(getRepoStringResult);
                response.Repo = repoResult;
            }
            #endregion

            #region GetBuildDefinition  
            var allbuildDefResponse = await _BuildService.GetAllBuildDefinition(projectName);
            BuildDefinition definition = null;
            if(allbuildDefResponse.IsSuccessStatusCode)
            {
                var buildContent = await allbuildDefResponse.Content.ReadAsStringAsync();
                var defStringResult = JsonConvert.DeserializeObject(buildContent).ToString();
                BuildDefinitionList defResults = JsonConvert.DeserializeObject<BuildDefinitionList>(defStringResult);
                
                var defList = defResults.Value.ToList();
                
                definition = defList.Where(def => def.Name.Contains(repoName)).FirstOrDefault();
                if(definition != null)
                {
                    var buildResponse = await _BuildService.GetBuildDefinition(definition.Id, projectName);
                    if(buildResponse.IsSuccessStatusCode)
                    {
                        var getBuidResponse = await buildResponse.Content.ReadAsStringAsync();
                        var getBuildStringResult = JsonConvert.DeserializeObject(getBuidResponse).ToString();
                        var buildResult = JsonConvert.DeserializeObject<BuildDefinition>(getBuildStringResult);

                        response.Build = buildResult;
                    }
                }
            } 
            #endregion

            #region GetReleaseDefinition       
            var allReleaseDefResponse = await _ReleaseService.GetReleaseDefinitions(projectName);
            ReleaseDefinition releaseDefinition = null;
            if (allReleaseDefResponse.IsSuccessStatusCode)
            {
                var releaseContent = await allReleaseDefResponse.Content.ReadAsStringAsync();
                var relStringResult = JsonConvert.DeserializeObject(releaseContent).ToString();
                ReleaseDefinitionList releaseResults = JsonConvert.DeserializeObject<ReleaseDefinitionList>(relStringResult);

                var releaseList = releaseResults.Value.ToList();

                releaseDefinition = releaseList.Where(rel => rel.Name.Contains(repoName)).FirstOrDefault();
                if(releaseDefinition != null)
                {
                    var releaseDelResponse = await _ReleaseService.GetReleaseDefinition(projectName, releaseDefinition.Id);
                    if(releaseDelResponse.IsSuccessStatusCode)
                    {
                        var getReleaseContent = await releaseDelResponse.Content.ReadAsStringAsync();
                        var releaseStringResult = JsonConvert.DeserializeObject(getReleaseContent).ToString();
                        var releaseResult = JsonConvert.DeserializeObject<ReleaseDefinition>(releaseStringResult);
                        response.Release = releaseResult;
                    }
                }
            }
            #endregion              
            return new OkObjectResult(response);
        }
    }
}