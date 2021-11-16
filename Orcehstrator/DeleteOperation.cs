using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using DevOps.TaskMaster.Orchestrator.Shared.Models;
using Newtonsoft.Json;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Enums;

namespace DevOps.TaskMaster.Orchestrator
{
    public partial class Orchestrator
    {
        [FunctionName("deleteOperation")]
      
        [OpenApiOperation(operationId: "deleteRepo", tags: new[] { "deleterepo" }, Summary = "delete repo", Description = "This Api will delete repo with the build and the release associated with it", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "projectName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "The project name", Description = "provide the project name eg: CHK", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "repoName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "The repo name", Description = "provide the repository name eg: tangodemo2", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(DeletedResponse), Summary = "The response", Description = "This returns the response")]

        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "repos/{projectName}/{repoName}")] HttpRequest request, string projectName, string repoName,
        ILogger log)
        {
            DeletedResponse DeletedResponse = new DeletedResponse();
            #region Delete Repo
            var deleteResponse = await _RepoService.DeleteRepository(projectName, repoName);
            if (deleteResponse.IsSuccessStatusCode)
            {
                var responseContent = await deleteResponse.Content.ReadAsStringAsync();
                var deleteStringResult = JsonConvert.DeserializeObject(responseContent).ToString();
                var deletedRepoResult = JsonConvert.DeserializeObject<RepoDeleteResponse>(deleteStringResult);
                
                DeletedResponse.Repo = deletedRepoResult;
            }
            #endregion

            #region Delete Build  
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
                    var buildDelResponse = await _BuildService.DeleteBuildDefinition(definition.Id, projectName);
                    if(buildDelResponse.IsSuccessStatusCode)
                    {
                        var delResponse = await buildDelResponse.Content.ReadAsStringAsync();
                        var delStringResult = JsonConvert.DeserializeObject(delResponse).ToString();
                        var deletedBuildResult = JsonConvert.DeserializeObject<RepoDeleteResponse>(delStringResult);

                        DeletedResponse.Build = deletedBuildResult;
                    }
                }
                else
                {
                    RepoDeleteResponse buildResponse = new RepoDeleteResponse()
                    {
                        Name = null,
                        Deleted = false,
                        Error = new Error() { Message = $"there's no build definition associated with repository {repoName}", Type = "DeleteBuildDefinition" }
                    };
                    DeletedResponse.Build = buildResponse;
                }
            } 
            #endregion   

            #region Delete Release       
            var allReleaseDefResponse = await _ReleaseService.GetReleaseDefinitions(projectName);
            ReleaseDefinition releaseDefinition = null;
            if (allReleaseDefResponse.IsSuccessStatusCode)
            {
                var releaseContent = await allReleaseDefResponse.Content.ReadAsStringAsync();
                var relStringResult = JsonConvert.DeserializeObject(releaseContent).ToString();
                ReleaseDefinitionList releaseResults = JsonConvert.DeserializeObject<ReleaseDefinitionList>(relStringResult);

                var releaseList = releaseResults.Value.ToList();

                releaseDefinition = releaseList.Where(rel => rel.Name.Contains(repoName)).FirstOrDefault();
                if (releaseDefinition != null)
                {
                    var releaseDelResponse = await _ReleaseService.DeleteReleaseDefinition(releaseDefinition.Id, releaseDefinition.Name, projectName);
                    if (releaseDelResponse.IsSuccessStatusCode)
                    {
                        var delResponseContent = await releaseDelResponse.Content.ReadAsStringAsync();
                        var releaseStringResult = JsonConvert.DeserializeObject(delResponseContent).ToString();
                        var deletedReleaseResult = JsonConvert.DeserializeObject<RepoDeleteResponse>(releaseStringResult);
                        DeletedResponse.Release = deletedReleaseResult;
                    }
                }
                else
                {
                    RepoDeleteResponse releaseResponse = new RepoDeleteResponse()
                    {
                        Name = null,
                        Deleted = false,
                        Error = new Error() { Message = $"there's no release definition associated with repository {repoName}", Type = "DeleteReleaseDefinition" }
                    };
                    DeletedResponse.Release = releaseResponse;
                }
            }
            #endregion              
            return new OkObjectResult(DeletedResponse);
        }
    }
 }