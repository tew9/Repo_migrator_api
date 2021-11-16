using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevOps.Repo.Api.Shared.Services;
using DevOps.Repo.Api.Shared.Mappers;
using DevOps.Repo.Api.Shared.Models.Git;
using DevOps.Repo.Contracts;

namespace DevOps.Repo.Api
{
  public class DeleteRepoFunc
  {
    #region Function Variables
    private IRepoService _repoService;
    private IGlobalMapper<Repository, RepoDto> _repoMapper;
    #endregion
    #region Constructor
    public DeleteRepoFunc(IRepoService repoService, IGlobalMapper<Repository, RepoDto> repoMapper)
    {
      _repoService = repoService;
      _repoMapper = repoMapper;
    }
    #endregion
    #region Function
    [FunctionName("DeleteRepo")]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "repos/{projectName}/{repoName}")] HttpRequest request, string    projectName, string repoName, ILogger log)
    {
      log.LogInformation("DeleteRepo Service Bus is triggered...");

      string responseMessage;
      
      if (string.IsNullOrEmpty(repoName))
      {
        responseMessage = "repoName is not provided, Please Provide it in your query params";
        return new OkObjectResult(responseMessage);
      }
      if (string.IsNullOrEmpty(projectName))
      {
        responseMessage = "projectName is not provided, Please Provide it in your query params";
        return new OkObjectResult(responseMessage);
      }
      #region SearchRepo
      var repo = await _repoService.GetRepository(projectName, repoName);
      var repoDto = _repoMapper.Map(repo);
      if (repo.Error != null)
      {
        responseMessage = JsonConvert.SerializeObject(repoDto.Error);
        return new OkObjectResult(responseMessage);
      }
      #endregion
      #region DeleteRepo
      var deletedRepo = await _repoService.DeleteRepo(repoDto);
      #endregion
      responseMessage = JsonConvert.SerializeObject(deletedRepo);
      return new OkObjectResult(responseMessage);
    }
    #endregion
  }
}
