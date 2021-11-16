using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using DevOps.Repo.Api.Shared.Mappers;
using DevOps.Repo.Api.Shared.Models.Git;
using DevOps.Repo.Contracts;
using Newtonsoft.Json;

namespace DevOps.Repo.Api.Shared.Services
{
  public class TfvcRepoService: ITfvcRepoService
  {
    #region Service Instances
    private readonly string _azUrl = Environment.GetEnvironmentVariable("AzDvoUrl");
    private readonly string _azUser = Environment.GetEnvironmentVariable("AzDvoUser");
    private readonly string _apiEndpoint = Environment.GetEnvironmentVariable("AzDvoApiEndpoint");
    private readonly string _apiVersion = Environment.GetEnvironmentVariable("AzDvoApiVersion");
    private readonly string _personalAccessToken = Environment.GetEnvironmentVariable("AzDvoPersonalAccessToken");
    private readonly string _apiVersionPreview = Environment.GetEnvironmentVariable("AzDvoApiVersionPreview");

    private IGlobalMapper<TfvcFolder, TfvcFolderDto> _folderMapper;
    private IGlobalMapper<GitImportTFVCSource, GitImportTFVCSourceDto> _importRequestMapper;
    private HttpClient _httpClient;
    #endregion 
    #region Properties
    private string ImportRepoRequest 
    { 
      get{return $"{_azUrl}/{{0}}/{_apiEndpoint}git/repositories/{{1}}/importRequests{_apiVersionPreview}";}
    }
    #endregion
    #region Constructors
    public TfvcRepoService(IGlobalMapper<TfvcFolder,TfvcFolderDto> folderMapper, 
      IGlobalMapper<GitImportTFVCSource, GitImportTFVCSourceDto> importRequestMapper)
    {
      _folderMapper = folderMapper;
      _importRequestMapper = importRequestMapper;
      _httpClient = new HttpClient();
      
      _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
      Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", _azUser, _personalAccessToken))));
    }
    #endregion
   
   
    #region TfvcImportRequest
    public async Task<GitImportTFVCSourceDto> TfvcImportRequest(TfvcParametersDto param, string destRepoId, string projectName)
    {
      if (param == null)
      {
        return new GitImportTFVCSourceDto(){ Error = new ErrorDto(){ Message = "Parameters cannot be null", Type="TfvcImportRequest"}};
      }
      if (string.IsNullOrEmpty(destRepoId))
      {
        return new GitImportTFVCSourceDto(){ Error = new ErrorDto(){ Message = "Target RepoId cannot be empty", Type="TfvcImportRequest"}};
      }
      if (string.IsNullOrEmpty(projectName))
      {
        projectName = "CHK";
      }

      TfvcFolderDto folder = await GetTfvcFolder(param.TfvcSource.path, projectName);
      if(folder.Error != null)
      {
        string message;
        try
        {
          message = folder.Error.Message;
        }
        catch (Exception)
        {
          message = "No error message provided";
        }
        return new GitImportTFVCSourceDto() { Error = new ErrorDto() { Message = message, Type="TFVCImportRequest" }};
      }

      var parameters = new TfvcParametersDto ()
      {
        DeleteServiceEndpointAfterImportIsDone = param.DeleteServiceEndpointAfterImportIsDone,
        TfvcSource = new TfvcSourceDto()
        {
          path = folder.Path,
          importHistory = param.TfvcSource.importHistory,
          importHistoryDurationInDays = param.TfvcSource.importHistoryDurationInDays
        }
      };
      GitImportTFVCSourceDto importRequest = new GitImportTFVCSourceDto()
      {
        Parameters = parameters
      };

      string endpoint = string.Format(ImportRepoRequest, projectName, destRepoId);
      
      //https://dev.azure.com/chkenergy/CHK/_apis/git/repositories/$($repoId)/importRequests?api-version=6.0-preview.1"
      HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(endpoint, importRequest);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var importRequestResponse = JsonConvert.DeserializeObject<GitImportTFVCSource>(responseContent);
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
        return new GitImportTFVCSourceDto() { Error = new ErrorDto() { Message = message, Type="TFVCImportRequest" }};
      }
    }
    #endregion
    #region GetOneFolder
    public async Task<TfvcFolderDto> GetTfvcFolder(string folderPath, string projectName)
    {
      if (string.IsNullOrEmpty(folderPath))
      {
        return new TfvcFolderDto() { Error = new ErrorDto() { Message = "Path cannot be null, Please, provide path eg:'yyy/xxx'", Type="GetTfvcFolder" }};
      }
      if (string.IsNullOrEmpty(projectName))
      {
        return new TfvcFolderDto() { Error = new ErrorDto() { Message = "Project name cannot be null, Please, provide project name", Type="GetTfvcFolder" }};
      }

      string endpoint = $"{_azUrl}{projectName}/{_apiEndpoint}tfvc/items/{folderPath}";

      var responseMessage = _httpClient.GetAsync(endpoint).GetAwaiter().GetResult();

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var folder = JsonConvert.DeserializeObject<TfvcFolder>(responseContent);
        var folderDto = _folderMapper.Map(folder);
        return folderDto;
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
        return new TfvcFolderDto() { Error = new ErrorDto() { Message = message, Type="GetTfvcFolder" }};
      };
    }
    #endregion
    #region GetAllTFVCFolder
    public async Task<TfvcFolderList> GetAllFolders(string projectName)
    {
      if (string.IsNullOrEmpty(projectName))
      {
        return null;
      }

      string endpoint = $"{_azUrl}{projectName}/{_apiEndpoint}tfvc/items";

      var responseMessage = _httpClient.GetAsync(endpoint).GetAwaiter().GetResult();

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var folder = JsonConvert.DeserializeObject<TfvcFolderList>(responseContent);
        return folder;
      }
      else 
      {
        return new TfvcFolderList() { Error = new ErrorDto() { Message = $"There are no tfvc folders in {projectName}, Please make sure the project name is correct or it exist!", Type = "GetAllFolders" }};
      };
    }
    #endregion
  }
}