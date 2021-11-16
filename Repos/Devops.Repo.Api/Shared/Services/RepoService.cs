using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using DevOps.Repo.Api.Shared.Mappers;
using DevOps.Repo.Api.Shared.Models.Git;
using DevOps.Repo.Contracts;
using DevOps.Repo.GitAutomation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DevOps.Repo.Api.Shared.Services
{
  public class RepoService: IRepoService
  {
    #region Service Instances
    private readonly string _azUrl = Environment.GetEnvironmentVariable("AzDvoUrl");
    private readonly string _apiEndpoint = Environment.GetEnvironmentVariable("AzDvoApiEndpoint");
    private readonly string _apiVersion = Environment.GetEnvironmentVariable("AzDvoApiVersion");
    private readonly string _personalAccessToken = Environment.GetEnvironmentVariable("AzDvoPersonalAccessToken");
    private readonly string _apiVersionPreview = Environment.GetEnvironmentVariable("AzDvoApiVersionPreview");
    private readonly string _templateUrl = Environment.GetEnvironmentVariable("templateUrl");
    private readonly string _committerEmail = Environment.GetEnvironmentVariable("committerEmail");
    

    private IGlobalMapper<Repository, RepoDto> _repoMapper;
    private IGlobalMapper<Item, RepoItemDto> _repoItemMapper;
    private IGlobalMapper<ImportRequest, ImportRequestDto> _importRequestMapper;
    private IGlobalMapper<PushResponse, RepoPushDto> _repoPushMapper;
    private IGitAutomationService _gitService;
    private IFileService _fileService;
    private HttpClient _httpClient;
    #endregion 
    #region Properties
    private string BaseUrl {get {return $"{_azUrl}{{0}}/{_apiEndpoint}";}}
    private string ImportRequestUrl
    {
      get{return $"git/repositories/{{0}}/importRequests{_apiVersionPreview}";}
    }
    private string ImportStatusRequestUrl 
    {
      get {return $"git/repositories/{{0}}/importRequests/{{1}}{_apiVersionPreview}";}
    }
    private string UpdateRepoFolderRequestUrl
    {
      get { return $"git/repositories/{{0}}/pushes{_apiVersion}";}
    }
    private string GetRepoRequestUrl
    {
      get { return $"git/repositories/{{0}}{_apiVersion}";}
    }
    private string GetAllRepoRequestUrl
    {
      get { return $"git/repositories/{_apiVersion}";}
    }
    private string DeleteRequestUrl
    {
      get {return $"{_apiEndpoint}git/repositories/{{0}}{_apiVersion}";}
    }
    private string CreateRepoRequestUrl
    {
      get {return $"git/repositories{_apiVersion}";}
    }
    #endregion

    #region Constructors
    public RepoService(IGlobalMapper<Repository, RepoDto> repoMapper, 
      IGlobalMapper<ImportRequest, ImportRequestDto> importRequestMapper, IGlobalMapper<Item, RepoItemDto> repoItemMapper, IFileService fileService, IGlobalMapper<PushResponse, RepoPushDto> repoPushMapper, IGitAutomationService gitService)
    {
      _repoMapper = repoMapper;
      _gitService = gitService;
      _importRequestMapper = importRequestMapper;
      _repoItemMapper = repoItemMapper;
      _repoPushMapper = repoPushMapper;
      _fileService = fileService;
      _httpClient = new HttpClient();
      
      _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
      Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken))));
    }
    #endregion
    #region CreateRepo
    public async Task<RepoDto> CreateRepo(string appName, ProjectDto project, string templateRepoName)
    {
      if (string.IsNullOrEmpty(appName))
      {
        return new RepoDto() { Error = new ErrorDto() { Message = "'RepoName' cannot be empty", Type = "CreateRepo" } };
      }
      if (project == null)
      {
        return new RepoDto() { Error = new ErrorDto() { Message = "'Project'  cannot be empty", Type = "CreateRepo" } };
      }
      if (string.IsNullOrEmpty(templateRepoName))
      {
        return new RepoDto() { Error = new ErrorDto() { Message = "'templateName' cannot be empty", Type = "CreateRepo" } };
      }

      string endpoint = $"{_azUrl}{_apiEndpoint}{CreateRepoRequestUrl}";

      string repoName = templateRepoName.Replace("UniqueNameGoesHere", appName);
      repoName = repoName.Replace("uniquenamegoeshere", appName.ToLower());

      Repository repo = new Repository()
      {
        Name = repoName,
        Project = project
      };

      HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(endpoint, repo);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var repoResponse = JsonConvert.DeserializeObject<Repository>(responseContent);
        var repoDto = _repoMapper.Map(repoResponse);

        return repoDto;
      }
      else
      {
        string message;

        try
        {
          message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
        }
        catch (Exception)
        {
          message = "No error message provided";
        }

        return new RepoDto()
        {
          Error = new ErrorDto()
          {
            Message = message,
            Type = "CreateRepo"
          }
        };
      }
    }
    #endregion   
    #region ImportRepo
    public async Task<ImportRequestDto> ImportRepo(string repoId, ServiceEndpointDto serviceEndpoint, string projectName, bool deleteServiceEndpointAfterImportIsDone = true)
    {
      if (serviceEndpoint == null)
      {
        return new ImportRequestDto() { Error = new ErrorDto() { Message = "'serviceEndpoint' cannot be empty", Type = "ImportRepo" } };
      }
      if (string.IsNullOrEmpty(repoId))
      {
        return new ImportRequestDto() { Error = new ErrorDto() { Message = "'repoid' cannot be empty", Type = "ImportRepo" } };
      }
      if (string.IsNullOrEmpty(projectName))
      {
        return new ImportRequestDto() { Error = new ErrorDto() { Message = "'projectName' cannot be empty", Type = "ImportRepo" } };
      }

      string endpoint = $"{string.Format(BaseUrl, projectName)}{string.Format(ImportRequestUrl, repoId)}";

      ParametersDto parameters = new ParametersDto()
      {
        GitSource = new GitSourceDto(){ Url = serviceEndpoint.Url},
        ServiceEndpointId = serviceEndpoint.Id,
        DeleteServiceEndpointAfterImportIsDone = deleteServiceEndpointAfterImportIsDone
      };

      ImportRequest importRequest = new ImportRequest()
      {
        Parameters = parameters
      };

      HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(endpoint, importRequest);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var importRequestResponse = JsonConvert.DeserializeObject<ImportRequest>(responseContent);
        var importRequestDto = _importRequestMapper.Map(importRequestResponse);

        return importRequestDto;
      }
      else
      {
        string message;

        try
        {
          message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
        }
        catch (Exception)
        {
          message = "No error message provided";
        }
        return new ImportRequestDto()
        {
          Error = new ErrorDto()
          {
            Message = message,
            Type = "ImportRepo"
          }
        };
      }
    }
    #endregion
    #region GetImportRepoStatus
    public async Task<string> GetImportRepoStatus(string repoId, string importRequestId, string projectName)
    {
      if (string.IsNullOrEmpty(repoId))
      {
        return null;
      }
      if (string.IsNullOrEmpty(importRequestId))
      {
        return null;
      }
      if (string.IsNullOrEmpty(projectName))
      {
        return null;
      }

      string endpoint = $"{string.Format(BaseUrl, projectName)}{string.Format(ImportStatusRequestUrl, repoId, importRequestId)}";

      HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var importRequestResponse = JsonConvert.DeserializeObject<ImportRequest>(responseContent);

        return importRequestResponse.Status;
      }

      return null;
    }  
    #endregion
    #region RepoPush
    public async Task<List<RepoPushDto>> UpdateRepoFilesAndFolders(string repoId, string applicationName, string projectName)
    {
      int recursion = 4;
      var repoPushDtos = new List<RepoPushDto>();

      repoPushDtos.AddRange(await UpdateRepoFolderNames(repoId, applicationName, projectName, recursion));
      repoPushDtos.Add(await UpdateRepoFileNames(repoId, applicationName, projectName));
      repoPushDtos.Add(await UpdateRepoItemContent(repoId, applicationName, projectName));

      return repoPushDtos;
    }
    #endregion
    #region UpdateRepoFolderNames
    private async Task<List<RepoPushDto>> UpdateRepoFolderNames(string repoId, string applicationName, string projectName, int recursion)
    {
      var repoPushes = new List<RepoPushDto>();

      if (string.IsNullOrEmpty(repoId))
      {
        var repoPush = new RepoPushDto() { Error = new ErrorDto() { Message = "'repoid' cannot be empty", Type = "UpdateRepoFolderNames" } };
        repoPushes.Add(repoPush);
        return repoPushes;
      }
      if (string.IsNullOrEmpty(applicationName))
      {
        var repoPush = new RepoPushDto() { Error = new ErrorDto() { Message = "'applicationName' cannot be empty", Type = "UpdateRepoFolderNames" } };
        repoPushes.Add(repoPush);
        return repoPushes;
      }
      for (int i = 1; i <= recursion; i++)
      {
        var repoItems = await GetRepoItems(repoId, projectName);

        if (repoItems.Count == 0)
        {
          var repoPush = new RepoPushDto()
          {
            Error = new ErrorDto()
            {
              Message = "No items were pulled from repo, id: " + repoId,
              Type = "UpdateRepoFolderNames"
            }
          };
          repoPushes.Add(repoPush);
          return repoPushes;
        }
        var changes = new List<Change>();

        foreach (var repoItem in repoItems)
        {
          int folderLevel = repoItem.Path.Count(x => x == '/');
          if (folderLevel == i && repoItem.Path.Contains("UniqueNameGoesHere") && repoItem.IsFolder)
          {
            var change = _fileService.RenameItem(repoItem.Path, applicationName);
            changes.Add(change);
          }
        }

        if (changes.Count == 0)
        {
          var repoPush = new RepoPushDto() { Name = "FolderRenameLevel" + i };
          repoPushes.Add(repoPush);
          continue;
        }

        string endpoint = $"{string.Format(BaseUrl, projectName)}{string.Format(UpdateRepoFolderRequestUrl, repoId)}";

        var refUpdates = new List<RefUpdate>();
        var commits = new List<Commit>();
        var refUpdate = await GetMasterRefUpdate(repoId, projectName);

        refUpdates.Add(refUpdate);

        commits.Add(new Commit()
        {
          Comment = "Renaming folder",
          Changes = changes
        });

        Push push = new Push()
        {
          RefUpdates = refUpdates,
          Commits = commits
        };

        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(endpoint, push);

        if (responseMessage.IsSuccessStatusCode)
        {
          var responseContent = await responseMessage.Content.ReadAsStringAsync();
          var pushResponse = JsonConvert.DeserializeObject<PushResponse>(responseContent);
          var repoPushDto = _repoPushMapper.Map(pushResponse);
          repoPushDto.Name = "FolderRenameLevel" + i;

          repoPushes.Add(repoPushDto);
        }
        else
        {
          string message;

          try
          {
            message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
          }
          catch (Exception)
          {
            message = "No error message provided";
          }

          repoPushes.Add(new RepoPushDto()
          {
            Error = new ErrorDto()
            {
              Message = message,
              Type = "UpdateRepoFolderNames"
            }
          });

          return repoPushes;
        }
      }
      return repoPushes;
    }
    #endregion
    #region UpdateFileNames
    private async Task<RepoPushDto> UpdateRepoFileNames(string repoId, string applicationName, string projectName)
    {
      if (string.IsNullOrEmpty(repoId))
      {
        return new RepoPushDto() { Error = new ErrorDto() { Message = "'repoId' cannot be empty", Type = "UpdateRepoFileNames" } };
      }
      if (string.IsNullOrEmpty(applicationName))
      {
        return new RepoPushDto() { Error = new ErrorDto() { Message = "'applicationName' cannot be empty", Type = "UpdateRepoFileNames" } };
      }
      if (string.IsNullOrEmpty(projectName))
      {
        return new RepoPushDto() { Error = new ErrorDto() { Message = "'projectName' cannot be empty", Type = "UpdateRepoFileNames" } };
      }

      var repoItems = await GetRepoItems(repoId, projectName);

      if (repoItems.Count == 0)
      {
        return new RepoPushDto()
        {
          Error = new ErrorDto()
          {
            Message = "No items were pulled from repo, id: " + repoId,
            Type = "UpdateRepoFileNames"
          }
        };
      }

      var changes = new List<Change>();

      foreach (var repoItem in repoItems)
      {
        if ((repoItem.Path.Contains("UniqueNameGoesHere") || repoItem.Path.Contains("uniquenamegoeshere")) && !repoItem.IsFolder)
        {
          var change = _fileService.RenameItem(repoItem.Path, applicationName);
          changes.Add(change);
        }
      }

      if (changes.Count == 0)
      {
        return new RepoPushDto() { Name = "FileRename" };
      }

      string endpoint = _azUrl + _apiEndpoint + "git/repositories/" + repoId + "/pushes" + _apiVersion;

      var refUpdates = new List<RefUpdate>();
      var commits = new List<Commit>();
      var refUpdate = await GetMasterRefUpdate(repoId, projectName);

      refUpdates.Add(refUpdate);

      commits.Add(new Commit()
      {
        Comment = "Renaming file",
        Changes = changes
      });

      Push push = new Push()
      {
        RefUpdates = refUpdates,
        Commits = commits
      };

      HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(endpoint, push);

       if (responseMessage.IsSuccessStatusCode)
        {
          var responseContent = await responseMessage.Content.ReadAsStringAsync();
          var pushResponse = JsonConvert.DeserializeObject<PushResponse>(responseContent);
          var repoPushDto = _repoPushMapper.Map(pushResponse);
          repoPushDto.Name = "FileRename";

          return repoPushDto;
        }
        else
        {
          string message;
          try
          {
            message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
          }
          catch (Exception)
          {
            message = "No error message provided";
          }
          return new RepoPushDto()
          {
            Error = new ErrorDto()
            {
              Message = message,
              Type = "UpdateFileRepoNames"
            }
          };
        }

    }
    #endregion
    #region UpdateRepoItemContents
    private async Task<RepoPushDto> UpdateRepoItemContent(string repoId, string applicationName, string projectName)
    {
      if (string.IsNullOrEmpty(repoId))
      {
        return new RepoPushDto() { Error = new ErrorDto() { Message = "'repoId' cannot be empty", Type = "UpdateRepoItemContent" } };
      }
      if (string.IsNullOrEmpty(applicationName))
      {
        return new RepoPushDto() { Error = new ErrorDto() { Message = "'applicationName' cannot be empty", Type = "UpdateRepoItemContent" } };
      }
      if (string.IsNullOrEmpty(projectName))
      {
        return new RepoPushDto() { Error = new ErrorDto() { Message = "'projectName' cannot be empty", Type = "UpdateRepoItemContent" } };
      }

      var repoItems = await GetRepoItems(repoId, projectName);

       if (repoItems.Count == 0)
      {
        return new RepoPushDto()
        {
          Error = new ErrorDto()
          {
            Message = "No items were pulled from repo, id: " + repoId,
            Type = "UpdateRepoItemContent"
          }
        };
      }

      var changes = new List<Change>();

      foreach (var repoItem in repoItems)
      {
        if (!repoItem.IsFolder)
        {
          string endpointA = _azUrl + projectName + "/" + _apiEndpoint + "git/repositories/" + repoId + "/items" +
                _apiVersion + "&path=" + repoItem.Path + "&includeContent=true";

          HttpResponseMessage responseMessageA = await _httpClient.GetAsync(endpointA);

          if (responseMessageA.IsSuccessStatusCode)
          {
            var responseContentA = await responseMessageA.Content.ReadAsStringAsync();
            var repoItemResponse = JsonConvert.DeserializeObject<Item>(responseContentA);

            if (repoItemResponse.Content.Contains("UniqueNameGoesHere") || repoItemResponse.Content.Contains("uniquenamegoeshere"))
            {
              Change change;
              change = _fileService.FindReplaceContent(repoItem.Path, repoItemResponse.Content, applicationName);
              changes.Add(change);
            }
          }
          else
          {
            string message;

            try
            {
              message = responseMessageA.Content.ReadAsAsync<HttpError>().Result.Message;
            }
            catch (Exception)
            {
              message = "No error message provided";
            }

            return new RepoPushDto()
            {
              Error = new ErrorDto()
              {
                Message = message,
                Type = "UpdateRepoItemContent"
              }
            };
          }
        }
      }
      if (changes.Count == 0)
      {
        return new RepoPushDto() { Name = "ContentReplace" };
      }

      string endpointB = _azUrl + _apiEndpoint + "git/repositories/" + repoId + "/pushes" + _apiVersion;

      var refUpdates = new List<RefUpdate>();
      var commits = new List<Commit>();
      var refUpdate = await GetMasterRefUpdate(repoId, projectName);

      refUpdates.Add(refUpdate);

      commits.Add(new Commit()
      {
        Comment = "Replacing file content",
        Changes = changes
      });

      Push push = new Push()
      {
        RefUpdates = refUpdates,
        Commits = commits
      };

      HttpResponseMessage responseMessageB = await _httpClient.PostAsJsonAsync(endpointB, push);

      if (responseMessageB.IsSuccessStatusCode)
      {
        var responseContentB = await responseMessageB.Content.ReadAsStringAsync();
        var pushResponse = JsonConvert.DeserializeObject<PushResponse>(responseContentB);
        var repoPushDto = _repoPushMapper.Map(pushResponse);
        repoPushDto.Name = "ContentReplace";

        return repoPushDto;
      }
      else
      {
        string message;

        try
        {
            message = responseMessageB.Content.ReadAsAsync<HttpError>().Result.Message;
        }
        catch (Exception)
        {
          message = "No error message provided";
        }

        return new RepoPushDto()
        {
          Error = new ErrorDto()
          {
            Message = message,
            Type = "UpdateRepoItemContent"
          }
        };
      }


    }  
    #endregion
    #region GetRepoItems
    private async Task<List<RepoItemDto>> GetRepoItems(string repoId, string projectName)
    {
      if (string.IsNullOrEmpty(repoId))
      {
        return null;
      }
      if (string.IsNullOrEmpty(projectName))
      {
        return null;
      }

      string endpoint = _azUrl + projectName + "/" + _apiEndpoint + "git/repositories/" + repoId + "/items?recursionLevel=full&" + _apiVersion;

      var repoItemDtos = new List<RepoItemDto>();

      HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var repoItemList = JsonConvert.DeserializeObject<ItemList>(responseContent);

        foreach (var repoItem in repoItemList.Value)
        {
          var repoItemDto = _repoItemMapper.Map(repoItem);
          repoItemDtos.Add(repoItemDto);
        }
      }

      return repoItemDtos;
    }
    #endregion
    #region GetMasterRefUpdate
    private async Task<RefUpdate> GetMasterRefUpdate(string repoId, string projectName)
    {
      if (string.IsNullOrEmpty(repoId))
      {
        return null;
      }
      if (string.IsNullOrEmpty(projectName))
      {
        return null;
      }

      string endpoint = _azUrl + projectName + "/" + _apiEndpoint + "git/repositories/" + repoId + "/refs" + _apiVersion;

      HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var refUpdates = JsonConvert.DeserializeObject<RefUpdateList>(responseContent);

        foreach (var refUpdate in refUpdates.Value)
        {
          if (refUpdate.Name == "refs/heads/master")
          {
            refUpdate.OldObjectId = refUpdate.ObjectId;
            return refUpdate;
          }
        }
      }

      return null;
    }
    #endregion
    #region GetOneRepo
    public async Task<Repository> GetRepository(string projectName, string repoName)
    {
      if (string.IsNullOrEmpty(repoName))
      {
        return null;
      }
      if (string.IsNullOrEmpty(projectName))
      {
        return null;
      }

      string endpoint = $"{string.Format(BaseUrl, projectName)}{string.Format(GetRepoRequestUrl, repoName)}";

      HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var repo = JsonConvert.DeserializeObject<Repository>(responseContent);
        return repo;
      }
      else 
      {
        string message;

        try
        {
          message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
        }
        catch (Exception)
        {
          message = "No error message provided";
        }
        return new Repository() { Error = new ErrorDto() { Message = message, Type = "GetRepo" }};
      };
    }
    #endregion
    #region DeleteRepo
    public async Task<RepoDeleteDto> DeleteRepo(RepoDto repo)
    {
      if (repo == null)
      {
        return new RepoDeleteDto() { Error = new ErrorDto() { Message = "'repository' cannot be null", Type = "DeleteRepo" } };
      }

      string endpoint = $"{_azUrl}{repo.Project.Id}/{string.Format(DeleteRequestUrl, repo.Id)}";

      HttpResponseMessage responseMessage = await _httpClient.DeleteAsync(endpoint);

      if (responseMessage.IsSuccessStatusCode)
      {
        return new RepoDeleteDto() { Name = repo.Name, Deleted = true };
      }
      else
      {
        string message;

        try
        {
          message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
        }
        catch (Exception)
        {
          message = "No error message provided";
        }

        return new RepoDeleteDto() { Error = new ErrorDto() { Message = message, Type = "DeleteRepo" } };
      }
    }
    #endregion
    #region CompleteRepoCreatiom
    #endregion
    #region GetAllRepo
    public async Task<RepoList> GetRepositories(string projectName)
    {
      var repos = new RepoList();
      if (string.IsNullOrEmpty(projectName))
      {
        return new RepoList () {
          Error = new ErrorDto(){ Message = "Please provide the project Name which you want to retrieve the repositories from", Type="GetAllRepos"}
        };
      }

      string endpoint = $"{string.Format(BaseUrl, projectName)}{GetAllRepoRequestUrl}";

      HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = responseMessage.Content.ReadAsStringAsync().Result;
        JObject jsonBody = JObject.Parse(responseContent);
        if (jsonBody["value"] != null && ((JArray)jsonBody["value"]).HasValues)
        {
          return JsonConvert.DeserializeObject<RepoList>(jsonBody.ToString());
        }
        else
        {
          string message;

          try
          {
            message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
          }
          catch (Exception)
          {
            message = "No error message provided";
          }
          return new RepoList () {
          Error = new ErrorDto(){ Message = message, Type="GetAllRepos"}
        };
        }
      }
      else 
      {
        return new RepoList () {
          Error = new ErrorDto(){ Message = "Something is wrong with the endpoints \nor there's no Repos in the Project you've provided!!!", Type="GetAllRepos"}
        };
      };
    }
    #endregion  
    #region CreateRepo
    public async Task<RepoDto> CreateRepo(string repoName, ProjectDto project)
    {
      if (string.IsNullOrEmpty(repoName))
      {
        return new RepoDto() { Error = new ErrorDto() { Message = "'repoName' cannot be empty", Type = "CreateRepoWithoutTemplate" } };
      }
      if (project == null)
      {
        return new RepoDto() { Error = new ErrorDto() { Message = "'Project'  cannot be empty", Type = "CreateRepoWithoutTemplate" } };
      }

      string endpoint = _azUrl + _apiEndpoint + "git/repositories" + _apiVersion;

      Repository repo = new Repository()
      {
        Name = repoName,
        Project = project
      };

      HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(endpoint, repo);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var repoResponse = JsonConvert.DeserializeObject<Repository>(responseContent);
        var repoDto = _repoMapper.Map(repoResponse);

        return repoDto;
      }
      else
      {
         string message;

        try
        {
          message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
        }
        catch (Exception)
        {
          message = "No error message provided";
        }

        return new RepoDto()
        {
          Error = new ErrorDto()
          {
            Message = message,
            Type = "CreateRepoWithoutTemplate"
          }
        };
      }
    }
    #endregion
    #region TemplateIntegration
    public async Task<Dictionary<string, string>> TemplateIntegration(string gitRemoteUrl, string[] extensions, string _workingdir, string committer,
                                                                      bool isCleanupAllowed, bool isJustIntegration)
    {
      var Message = new Dictionary<string, string>();
      if(string.IsNullOrEmpty(committer)) committer = _committerEmail; 
      if (string.IsNullOrEmpty(gitRemoteUrl))
      {
        var outPut =  new OutPut () {
          Error = new ErrorDto(){ Message = "Please provide repo remote URL", Type="TemplateIntegration"}
        };
        Message.Add("error", outPut.Error.Message);
        return Message;
      }

      if (extensions.Length <= 0)
      {
        var outPut =  new OutPut () {
          Error = new ErrorDto(){ Message = "Please provide the extension that needs to be cleaned, eg. vssscc for tfvc", Type="TemplateIntegration"}
        };
        Message.Add("error", outPut.Error.Message);
        return Message;
      }

      _gitService = new GitAutomationService(gitRemoteUrl, _templateUrl, _workingdir, _personalAccessToken, string.Empty, extensions, committer);
      var result = _gitService.IntegrateTemplate("Automated Integration of templates and Repo clean up.", isCleanupAllowed, isJustIntegration);
      return result;
    }
    #endregion
  }
}