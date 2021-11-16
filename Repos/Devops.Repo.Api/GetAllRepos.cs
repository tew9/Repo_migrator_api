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
  public class GetRepositoriesFunc
  {
    #region Function Variables
    private IRepoService _repoService;
    #endregion

    #region Constructor
    public GetRepositoriesFunc(IRepoService repoService)
    {
      _repoService = repoService;
    }
    #endregion

    #region Function
    [FunctionName("GetAllRepos")]
    public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "repos/{projectName}")] HttpRequest request, string projectName,
    ILogger log)
    {
      log.LogInformation("GetAllRepos is triggered ...!");
      string responseMessage;

      if (string.IsNullOrEmpty(projectName))
      {
        responseMessage = "please provide project Name...!";
        log.LogError("projectName wasn't provided");
        return new BadRequestObjectResult(responseMessage);
      }
      #region SearchRepo
      var repos = await _repoService.GetRepositories(projectName);
      #endregion
      if(repos == null)
      {
        log.LogError("something is wrong with the repo service...");
      }
      responseMessage = JsonConvert.SerializeObject(repos);
      log.LogInformation("function is completed");
      return new OkObjectResult(responseMessage);
    }
    #endregion
  }
}
