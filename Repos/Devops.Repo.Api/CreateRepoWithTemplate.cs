using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using DevOps.Repo.Contracts;
using DevOps.Repo.Api.Shared.Services;
using DevOps.Repo.Api.Shared.Models.Git;
using DevOps.Repo.Api.Shared.Mappers;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using DevOps.Repo.Api.Shared.Models.TableEntities;

namespace DevOps.Repo.Api
{
  public class CreatePushRepoFunc
  {
    #region Constants
    private ITemplateService _templateService;
    private IProjectService _projectService;
    private IRepoService _repoService;
    private IEndPointService _serviceEndpoint;
    IGlobalMapper<Repository, RepoDto> _repoMapper;
    IGlobalMapper<ApplicationTemplate, ApplicationTemplateDto> _templateMapper;
    private readonly string _workingdir;
    private readonly DriveInfo _driveInfo;
    #endregion
    #region Constructors    
    public CreatePushRepoFunc(ITemplateService templateService, IProjectService projectService, IRepoService repoService,
      IEndPointService endPointService, IGlobalMapper<Repository, RepoDto> repoMapper, IGlobalMapper<ApplicationTemplate, ApplicationTemplateDto> templateMapper)
    {
      _repoMapper = repoMapper;
      _templateMapper = templateMapper;
      _driveInfo = new DriveInfo("D"); //change drive name if your testing locally and path
      _workingdir = Path.Combine(_driveInfo.RootDirectory.FullName, "home\\data");
      _templateService = templateService;
      _projectService = projectService;
      _repoService = repoService;
      _serviceEndpoint = endPointService;
    }
    #endregion
    #region AzureFunction
    [FunctionName("CreateRepoWithTemplate")]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Function, "post", Route = "repos/templates")] HttpRequest request, ILogger log)
    {
      
      log.LogInformation("createRepo from template service bus is triggered..");

      string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
      dynamic applicationDto = JsonConvert.DeserializeObject<ApplicationDto>(requestBody);

      if (applicationDto == null)
      {
        return new BadRequestObjectResult("malformed request body, All fields Are required!!");
      }

      if (string.IsNullOrEmpty(applicationDto.DestinationRepoName))
      {
        return new BadRequestObjectResult("Reponame cannot be empty, Please Provide it as destinationRepoName");
      }
      if (string.IsNullOrEmpty(applicationDto.DestinationProjectName))
      {
        return new BadRequestObjectResult("project name cannot be empty, Please Provide it as destinationProjectName");
      }
      if (string.IsNullOrEmpty(applicationDto.TemplateName))
      {
        return new BadRequestObjectResult("TemplateName name cannot be empty, Please Provide it as templateName");
      }

      var createdRepositoryDto = new ApplicationDto()
      {
        DestinationRepoName = applicationDto.DestinationRepoName,
        TemplateName = applicationDto.TemplateName,
        DestinationProjectName = applicationDto.DestinationProjectName,
        Integration = applicationDto.Integration,
        CleanUp = applicationDto.CleanUp,
      };

      var output = new OutPut();

      var repoRequest = new ApplicationRequestDto()
      {
        RequestId = Guid.NewGuid().ToString(),
        AppType = createdRepositoryDto.TemplateName
      };

      createdRepositoryDto.Id = repoRequest.RequestId;
    
      #region getTemplate
      var template = await _templateService.GetTemplate(applicationDto.TemplateName);
      if (template == null)
      {
        createdRepositoryDto.Error = new ErrorDto()
        {
          Message = createdRepositoryDto.TemplateName + " is not available as a valid template type",
          Type = "GetApplicationTemplate"
        };
        createdRepositoryDto.Status = "Failed";
        output = new OutPut()
        {
          Status = createdRepositoryDto.Status,
          Error = createdRepositoryDto.Error
        };
        return new BadRequestObjectResult(output);
      }
      createdRepositoryDto.Template = _templateMapper.Map(template);
      #endregion

      #region getProject
      var project = await _projectService.GetProject(createdRepositoryDto.DestinationProjectName);
      if (project.Error != null)
      {
        createdRepositoryDto.Error = new ErrorDto() { Message = project.Error.Message, Type = project.Error.Type };
        createdRepositoryDto.Status = "failed";
        output = new OutPut()
        {
          Status = createdRepositoryDto.Status,
          Error = createdRepositoryDto.Error
        };
        return new BadRequestObjectResult(output);
      }
      createdRepositoryDto.Project = project;
      #endregion

      var existingRepo = await _repoService.GetRepository(createdRepositoryDto.DestinationProjectName,
                                                          createdRepositoryDto.DestinationRepoName);

      #region CreateRepo
      dynamic repo = null;  
      if (existingRepo.Error != null)
      {
        repo = await _repoService.CreateRepo(createdRepositoryDto.DestinationRepoName, project, template.RepoName);
        if (repo.Error != null)
        {
          createdRepositoryDto.Error = new ErrorDto() { Message = repo.Error.Message, Type = repo.Error.Type };
          createdRepositoryDto.Status = "failed";
          output = new OutPut()
          {
            Status = createdRepositoryDto.Status,
            Error = createdRepositoryDto.Error
          };
          return new BadRequestObjectResult(output);
        }

        createdRepositoryDto.Repo = repo;
      }
      #endregion

      #region CloneTemplate Into Existing empty repo
      else if (existingRepo.Size == 0)
      {
        createdRepositoryDto.Repo = _repoMapper.Map(existingRepo);
      }
      #endregion

      #region IntegrageTemplate into RepoWithFiles
      else
      {
        createdRepositoryDto.Repo = _repoMapper.Map(existingRepo);
        Dictionary<string, string> integrationResults = null;
        var extensions = new string[]{ "VSSSCC", "VSPSCC"};
        if(createdRepositoryDto.Integration && !createdRepositoryDto.CleanUp)
          integrationResults = await _repoService.TemplateIntegration(existingRepo.RemoteUrl, extensions, _workingdir, applicationDto.Email, false, true);
        
        else if(createdRepositoryDto.CleanUp && !createdRepositoryDto.Integration)
          integrationResults = await _repoService.TemplateIntegration(existingRepo.RemoteUrl, extensions, _workingdir, applicationDto.Email, true, false);

        else
          integrationResults = await _repoService.TemplateIntegration(existingRepo.RemoteUrl, extensions,_workingdir, applicationDto.Email, true, true);

        var result = new OutPut()
        {
          Status = "Complete",
          Repo = new RepoOut() { Id = createdRepositoryDto.Repo.Id, Name = createdRepositoryDto.Repo.Name },
          ProjectName = createdRepositoryDto.DestinationProjectName,
          Template = null,
          Integration = integrationResults
        };
        return new OkObjectResult(JsonConvert.SerializeObject(result));
      }  
      #endregion
     

      #region CheckIfServiceEndpointExist
      var serviceEndpoint = await _serviceEndpoint.CheckIfServiceEndpointExists(createdRepositoryDto.TemplateName, project.Name);
      if (serviceEndpoint == null)
      {
        serviceEndpoint = await _serviceEndpoint.CreateServiceEndpoint(createdRepositoryDto.GitSourceRepoName, project.Name, template.GitUrl);

        if (serviceEndpoint.Error != null)
        {
          createdRepositoryDto.Error = new ErrorDto() { Message = serviceEndpoint.Error.Message, Type = serviceEndpoint.Error.Type };
          createdRepositoryDto.Status = "failed";
          output = new OutPut()
          {
            Status = createdRepositoryDto.Status,
            Error = createdRepositoryDto.Error
          };
          return new BadRequestObjectResult(output);
        }
      }
      else if (serviceEndpoint.Error != null)
      {
        createdRepositoryDto.Error = new ErrorDto() { Message = serviceEndpoint.Error.Message, Type = serviceEndpoint.Error.Type };
        createdRepositoryDto.Status = "failed";
        output = new OutPut()
        {
          Status = createdRepositoryDto.Status,
          Error = createdRepositoryDto.Error
        };
        return new BadRequestObjectResult(output);
      }

      createdRepositoryDto.ServiceEndpoint = serviceEndpoint;
      #endregion
     
      #region ImportRepoRequest
      var importRequest = await _repoService.ImportRepo(createdRepositoryDto.Repo.Id, serviceEndpoint, project.Name, createdRepositoryDto.DeleteServiceEndpointAfterImportIsDone);

      if (importRequest.Error != null)
      {
        createdRepositoryDto.Error = new ErrorDto() { Message = importRequest.Error.Message, Type = importRequest.Error.Type };
        createdRepositoryDto.Status = "failed";
        output = new OutPut()
        {
          Status = createdRepositoryDto.Status,
          Error = createdRepositoryDto.Error
        };
        return new BadRequestObjectResult(output);
      }

      //Wait for the importRequest to be complete
      int statusChecks = 0;
      do
      {
        System.Threading.Thread.Sleep(1000);
        importRequest.Status = await _repoService.GetImportRepoStatus(createdRepositoryDto.Repo.Id, importRequest.ImportRequestId, project.Name);
        statusChecks++;
      } while (importRequest.Status != "completed" && statusChecks < 15);

      if (importRequest.Status != "completed")
      {
        createdRepositoryDto.Error = new ErrorDto() { Message = "importrepo request timed out", Type = "ImportRequest" };
        createdRepositoryDto.Status = "failed";
        output = new OutPut()
        {
          Status = createdRepositoryDto.Status,
          Error = createdRepositoryDto.Error
        };
        return new BadRequestObjectResult(output);
      }

      createdRepositoryDto.ImportRequest = importRequest;
      #endregion

      #region RepoPushFolders
      var repoPushes = await _repoService.UpdateRepoFilesAndFolders(createdRepositoryDto.Repo.Id, applicationDto.DestinationRepoName, project.Name);
      foreach (var repoPush in repoPushes)
      {
        if (repoPush.Error != null)
        {
          createdRepositoryDto.Error = new ErrorDto() { Message = repoPush.Error.Message, Type = repoPush.Error.Type };
          createdRepositoryDto.Status = "failed";
          output = new OutPut()
          {
            Status = createdRepositoryDto.Status,
            Error = createdRepositoryDto.Error
          };
          return new BadRequestObjectResult(output);
        }
      }

      createdRepositoryDto.RepoPushes = repoPushes; 
      #endregion
      createdRepositoryDto.Status = "completed";

      output = new OutPut()
      {
        Status = createdRepositoryDto.Status,
        Repo = new RepoOut(){Id = createdRepositoryDto.Repo.Id, Name =  createdRepositoryDto.Repo.Name},
        ProjectName =  createdRepositoryDto.DestinationProjectName,
        Template = new TemplateoOut()
        {
          BuildAgentName = createdRepositoryDto.Template.BuildAgentName, 
          TemplateBuildName = createdRepositoryDto.Template.BuildName,
          Name = createdRepositoryDto.Template.RowKey,
        }
      };
      string responseMessage = JsonConvert.SerializeObject(output);
      return new OkObjectResult(responseMessage);
    }
    #endregion
  }
}


