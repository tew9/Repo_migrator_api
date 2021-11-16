using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using DevOps.Repo.Api.Shared.Mappers;
using DevOps.Repo.Contracts;
using DevOps.Repo.Api.Shared.Models;
using Newtonsoft.Json;

namespace DevOps.Repo.Api.Shared.Services
{
  public class ProjectService: IProjectService
  {
    #region Instance Variables
    private readonly string _azUrl = Environment.GetEnvironmentVariable("AzDvoUrl");
    private readonly string _apiEndpoint = Environment.GetEnvironmentVariable("AzDvoApiEndpoint");
    private readonly string _apiVersion = Environment.GetEnvironmentVariable("AzDvoApiVersion");
    private readonly string _personalAccessToken = Environment.GetEnvironmentVariable("AzDvoPersonalAccessToken");
    HttpClient _httpClient;
    IGlobalMapper<Project, ProjectDto> _projectMapper;
    #endregion
    #region Properties
    private string BaseUrl {get {return $"{_azUrl}{_apiEndpoint}";}}
    private string ProjectRequestUrl
    {
      get{return $"projects/{{0}}{_apiVersion}";}
    }
    #endregion
    #region Constructor
    public ProjectService(IGlobalMapper<Project, ProjectDto> globalMapper, HttpClient client)
    {
      _httpClient = client;
      _projectMapper = globalMapper;

      _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
      Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken))));
    }
    #endregion

    #region GetProject
    public async Task<ProjectDto> GetProject(string projectName)
    {
      if(string.IsNullOrEmpty(projectName))
      {
        return new ProjectDto() { Error = new ErrorDto() { Message = "'name' cannot be empty", Type = "GetProject" } };
      }

      string endpoint = $"{BaseUrl}{string.Format(ProjectRequestUrl, projectName)}";
      
      HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var project = JsonConvert.DeserializeObject<Project>(responseContent);
        var projectDto = _projectMapper.Map(project);

        return projectDto;
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

        return new ProjectDto()
        {
          Error = new ErrorDto()
          {
            Message = message,
            Type = "GetProject"
          }
        };
      }
    }
    #endregion
  }
}