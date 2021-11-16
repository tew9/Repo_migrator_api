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
using DevOps.Repo.Api.Shared.Mappers;
using DevOps.Repo.Api.Shared.Models.Git;

namespace DevOps.Repo.Api
{
  public class UpdateRepositoryFunc
  {
    #region Constants
    private ITemplateService _templateService;
    private IProjectService _projectService;
    private IRepoService _repoService;
    private IEndPointService _serviceEndpoint;
    private ITfvcRepoService _tfvcRepoService;
    IGlobalMapper<Repository, RepoDto> _repoMapper;
    private readonly DriveInfo _driveInfo;
    private string _workingdir;

    #endregion
    #region Constructors    
    public UpdateRepositoryFunc(ITemplateService templateService, IProjectService projectService, ITfvcRepoService tfvcRepoService, IRepoService repoService,
      IEndPointService endPointService,IGlobalMapper<Repository, RepoDto> repoMapper)
    {
      _driveInfo = new DriveInfo("D"); //change drive name if your testing locally and path
      _workingdir = Path.Combine(_driveInfo.RootDirectory.FullName, "home\\data");
      _repoMapper = repoMapper;
      _templateService = templateService;
      _projectService = projectService;
      _repoService = repoService;
      _serviceEndpoint = endPointService;
      _tfvcRepoService = tfvcRepoService;
    }
    #endregion
    #region AzureFunction
    [FunctionName("MigrateRepoItems")]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Function, "put", Route = "repos")] HttpRequest request, ILogger log)
    {

      log.LogInformation("migration service bus is triggered with working directory as "+_workingdir);

      string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
      ApplicationDto applicationDto = JsonConvert.DeserializeObject<ApplicationDto>(requestBody);

      if (applicationDto == null)
      {
        return new BadRequestObjectResult("malformed request body, All fields Are required!!");
      }
      if (string.IsNullOrEmpty(applicationDto.RepoType))
      {
        return new BadRequestObjectResult("Repo type cannot be empty, it should be either 'git' or 'tfvc', Please Provide it as repoType");
      }
      if (string.IsNullOrEmpty(applicationDto.GitSourceRepoName) && applicationDto.RepoType == "git")
      {
        return new BadRequestObjectResult("Git Source Repository cannot be empty, Please provide it as sourceRepoName");
      }
      if (string.IsNullOrEmpty(applicationDto.DestinationProjectName))
      {
        return new BadRequestObjectResult("Destination project name cannot be empty, , Please provide it as destinationProjectName");
      }
      if (string.IsNullOrEmpty(applicationDto.DestinationRepoName))
      {
        return new BadRequestObjectResult("Destination Repository cannot be empty, Please provide it as destinationRepoName");
      }
      if (string.IsNullOrEmpty(applicationDto.SourceProjectName))
      {
        return new BadRequestObjectResult("Source project name cannot be empty, , Please provide it as sourceProjectName");
      }
      if (applicationDto.TfvcSource == null && applicationDto.RepoType == "tfvc")
      {
        return new BadRequestObjectResult("TfvcSource Object cannot be null, , Please provide it as tfvcSource and provide atleast Path Example tfvcSource:{\"Path\": \"your tfvc path\"}");
      }

      var createdRepositoryDto = new ApplicationDto()
      {
        DestinationRepoName = applicationDto.DestinationRepoName,
        DestinationProjectName = applicationDto.DestinationProjectName,
        SourceProjectName =  applicationDto.SourceProjectName,
        GitSourceRepoName = applicationDto.GitSourceRepoName,
        RepoType = applicationDto.RepoType,
        TfvcSource = applicationDto.TfvcSource,
        DeleteServiceEndpointAfterImportIsDone = applicationDto.DeleteServiceEndpointAfterImportIsDone
      };

      
      var repoRequest = new ApplicationRequestDto()
      {
        RequestId = Guid.NewGuid().ToString(),
      };

      createdRepositoryDto.Id = repoRequest.RequestId;
      dynamic output = null;
      #region getProject
      var destProject = await _projectService.GetProject(createdRepositoryDto.DestinationProjectName);

      if (destProject.Error != null)
      {
        createdRepositoryDto.Error = new ErrorDto() { Message = destProject.Error.Message, Type = destProject.Error.Type };
        createdRepositoryDto.Status = "failed"; 
        output = new OutPut()
        {
          Status = createdRepositoryDto.Status,
          Error = createdRepositoryDto.Error
        };
        return new BadRequestObjectResult(JsonConvert.SerializeObject(output));
      }
      createdRepositoryDto.Project = destProject;

      var sourceProject = await _projectService.GetProject(createdRepositoryDto.SourceProjectName);

      if (sourceProject.Error != null)
      {
        createdRepositoryDto.Error = new ErrorDto() { Message = sourceProject.Error.Message, Type = sourceProject.Error.Type };
        createdRepositoryDto.Status = "failed";
        output = new OutPut()
        {
          Status = createdRepositoryDto.Status,
          Error = createdRepositoryDto.Error
        };
        return new BadRequestObjectResult(JsonConvert.SerializeObject(output));
      }
      createdRepositoryDto.SourceProject = sourceProject;
      #endregion

      #region CheckDestinationRepo
      var destRepo = await _repoService.GetRepository(createdRepositoryDto.DestinationProjectName,
                                                      createdRepositoryDto.DestinationRepoName);

      if (destRepo.Error != null)
      {
        createdRepositoryDto.Error = new ErrorDto() { Message = destRepo.Error.Message, Type = destRepo.Error.Type };
        createdRepositoryDto.Status = "failed";
        output = new OutPut()
        {
          Status = createdRepositoryDto.Status,
          Error = createdRepositoryDto.Error
        };
        return new BadRequestObjectResult(JsonConvert.SerializeObject(output));
      }
      createdRepositoryDto.Repo = _repoMapper.Map(destRepo);
      #endregion

      dynamic importRequest = null;
      if (createdRepositoryDto.RepoType == "git")
      {
        #region MigrateGitToGit
        var sourceRepo = await _repoService.GetRepository(createdRepositoryDto.SourceProjectName, createdRepositoryDto.GitSourceRepoName);
        if (sourceRepo.Error != null)
        {
          createdRepositoryDto.Error = new ErrorDto() { Message = sourceRepo.Error.Message, Type = sourceRepo.Error.Type };
          createdRepositoryDto.Status = "failed";
          output = new OutPut()
          {
            Status = createdRepositoryDto.Status,
            Error = createdRepositoryDto.Error
          };
          return new BadRequestObjectResult(JsonConvert.SerializeObject(output));
        }

        createdRepositoryDto.SourceRepo = _repoMapper.Map(sourceRepo);
        #endregion

        
        #region CheckIfServiceEndpointExist
        var serviceEndpoint = await _serviceEndpoint.CheckIfServiceEndpointExists(createdRepositoryDto.GitSourceRepoName, destProject.Name);
        if (serviceEndpoint == null)
        {
          serviceEndpoint = await _serviceEndpoint.CreateServiceEndpoint(createdRepositoryDto.GitSourceRepoName, destProject.Name, sourceRepo.RemoteUrl);
          if (serviceEndpoint.Error != null)
          {
            createdRepositoryDto.Error = new ErrorDto() { Message = serviceEndpoint.Error.Message, Type = serviceEndpoint.Error.Type };
            createdRepositoryDto.Status = "failed";
            output = new OutPut()
            {
              Status = createdRepositoryDto.Status,
              Error = createdRepositoryDto.Error
            };
            return new BadRequestObjectResult(JsonConvert.SerializeObject(output));
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
          return new BadRequestObjectResult(JsonConvert.SerializeObject(output));
        }

        createdRepositoryDto.ServiceEndpoint = serviceEndpoint;
        #endregion

        importRequest = await _repoService.ImportRepo(destRepo.Id, serviceEndpoint, destProject.Name, createdRepositoryDto.DeleteServiceEndpointAfterImportIsDone);

        if (importRequest.Error != null)
        {
          createdRepositoryDto.Error = new ErrorDto() { Message = importRequest.Error.Message, Type = importRequest.Error.Type };
          createdRepositoryDto.Status = "failed";
          output = new OutPut()
          {
            Status = createdRepositoryDto.Status,
            Error = createdRepositoryDto.Error
          };
          return new BadRequestObjectResult(JsonConvert.SerializeObject(output));
        }
      }
      
      if (createdRepositoryDto.RepoType == "tfvc")
      {
        var parameters = new TfvcParametersDto()
        {
          DeleteServiceEndpointAfterImportIsDone = createdRepositoryDto.DeleteServiceEndpointAfterImportIsDone,
          TfvcSource = createdRepositoryDto.TfvcSource
        };

        importRequest = await _tfvcRepoService.TfvcImportRequest(parameters, createdRepositoryDto.Repo.Id, sourceProject.Name);
       
        if (importRequest.Error != null)
        {
          createdRepositoryDto.Error = new ErrorDto() { Message = importRequest.Error.Message, Type = importRequest.Error.Type };
          createdRepositoryDto.Status = "failed";
          output = new OutPut()
          {
            Status = createdRepositoryDto.Status,
            Error = createdRepositoryDto.Error
          };
          return new BadRequestObjectResult(JsonConvert.SerializeObject(output));
        }
      }

      #region Perfom the import
      //Wait for the importRequest to be complete
      int statusChecks = 0;
      do
      {
        System.Threading.Thread.Sleep(1000);
        importRequest.Status = await _repoService.GetImportRepoStatus(destRepo.Id, importRequest.ImportRequestId, 
        destProject.Name);
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
        return new BadRequestObjectResult(JsonConvert.SerializeObject(output));
      }
      
      if(createdRepositoryDto.RepoType == "tfvc")
      {
        createdRepositoryDto.TfvcImportRequest = importRequest;
      }

      if (createdRepositoryDto.RepoType == "git")
      {
        createdRepositoryDto.ImportRequest = importRequest;
      }
      #endregion
      
      #region RepoPushFolders
      var repoPushes = await _repoService.UpdateRepoFilesAndFolders(destRepo.Id, createdRepositoryDto.DestinationRepoName, destProject.Name);
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

      string[] extensions;
      dynamic result = null;
      if(createdRepositoryDto.RepoType == "tfvc")
      {
        extensions = new string[]{ "VSSSCC", "VSPSCC"};
        var integrationResults = await _repoService.TemplateIntegration(destRepo.RemoteUrl, extensions, _workingdir, applicationDto.Email, true, true);
        result = new OutPut()
        {
          Status = createdRepositoryDto.Status,
          Repo = new RepoOut() { Id = createdRepositoryDto.Repo.Id, Name = createdRepositoryDto.Repo.Name },
          ProjectName = createdRepositoryDto.DestinationProjectName,
          Integration = integrationResults
        };
      }
      else
      {
        result = new OutPut()
        {
          Status = createdRepositoryDto.Status,
          Repo = new RepoOut() { Id = createdRepositoryDto.Repo.Id, Name = createdRepositoryDto.Repo.Name },
          ProjectName = createdRepositoryDto.DestinationProjectName
        };
      }
      string responseMessage = JsonConvert.SerializeObject(result);
      return new OkObjectResult(responseMessage);
    }
    #endregion
  }
}


