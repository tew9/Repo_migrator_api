using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevOps.Repo.Api.Shared.Services;

namespace DevOps.Repo.Api
{
  public class GetRepositoryFunc
  {
    #region Function Variables
    private IRepoService _repoService;
    #endregion
    #region Constructor
    public GetRepositoryFunc(IRepoService repoService)
    {
      _repoService = repoService;
    }
    #endregion
    #region Function
    [FunctionName("GetOneRepo")]
    public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "repos/{projectName}/{repoName}")] HttpRequest request, string projectName, string repoName, 
    ILogger log)
    {
      log.LogInformation("GetOneRepo is triggered ...!");
      string responseMessage;
      if (string.IsNullOrEmpty(repoName))
      {
        responseMessage = "This HTTP triggered function executed successfully, but RepoName/Id is Required\n";
        return new OkObjectResult(responseMessage);
      }
      if (string.IsNullOrEmpty(projectName))
      {
        responseMessage = "This HTTP triggered function executed successfully, but projectName is Required\n";
        return new OkObjectResult(responseMessage);
      }
      #region SearchRepo
      var repo = await _repoService.GetRepository(projectName, repoName);
      #endregion
      responseMessage = JsonConvert.SerializeObject(repo);
      return new OkObjectResult(responseMessage);
    }
    #endregion
  }
}
