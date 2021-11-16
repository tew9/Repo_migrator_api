using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevOps.Repo.Contracts;
using DevOps.Repo.Api.Shared.Services;

namespace DevOps.Repo.Api
{
  public class CreateEmptyRepoFunc
  {
    #region Constants
    private IProjectService _projectService;
    private IRepoService _repoService;
    #endregion
  
    #region Constructors    
    public CreateEmptyRepoFunc(IProjectService projectService, IRepoService repoService,
      IEndPointService endPointService)
    {
      _projectService = projectService;
      _repoService = repoService;
    }
    #endregion
  
    #region AzureFunction
    [FunctionName("CreateEmptyRepo")]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Function, "post", Route = "repos")] HttpRequest request, ILogger log)
    {
      log.LogInformation("CreateEmptyRepo Service Bus is triggered ...");

      string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
      dynamic applicationDto = JsonConvert.DeserializeObject<ApplicationDto>(requestBody);

      if(applicationDto == null)
      {
        return new OkObjectResult("malformed request body, reponame and chk fields Are required!!");
      }

      if (string.IsNullOrEmpty(applicationDto.DestinationRepoName))
      {
        return new BadRequestObjectResult("Reponame cannot be empty, Please Provide it as destinationRepoName");
      }
      if (string.IsNullOrEmpty(applicationDto.DestinationProjectName))
      {
        return new BadRequestObjectResult("project name cannot be empty, Please Provide it as destinationProjectName");
      }

      var repoRequest = new ApplicationRequestDto()
      {
        RequestId = Guid.NewGuid().ToString(),
      };

      var createdRepositoryDto = new ApplicationDto()
      {
        DestinationRepoName = applicationDto.DestinationRepoName,
      };
      createdRepositoryDto.Id = repoRequest.RequestId;
     
      #region getProject
      var project = await _projectService.GetProject(applicationDto.DestinationProjectName);
    
      if (project.Error != null)
      {
        createdRepositoryDto.Error = new ErrorDto() { Message = project.Error.Message, Type = project.Error.Type };
        createdRepositoryDto.Status = "failed";
  
        return new BadRequestObjectResult(createdRepositoryDto);
      }
      createdRepositoryDto.Project = project;
      #endregion

      #region CreateRepo
      var repo = await _repoService.CreateRepo(createdRepositoryDto.DestinationRepoName, project);
      if (repo.Error != null)
      {
        createdRepositoryDto.Error = new ErrorDto() { Message = repo.Error.Message, Type = repo.Error.Type };
        createdRepositoryDto.Status = "failed";
        return new BadRequestObjectResult(createdRepositoryDto);
      }

      createdRepositoryDto.Repo = repo;
      #endregion

      createdRepositoryDto.Status = "completed";
      string responseMessage = JsonConvert.SerializeObject(createdRepositoryDto);
      return new OkObjectResult(responseMessage);
    }
    #endregion
  }
}