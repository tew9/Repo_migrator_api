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
  public class EndPointService: IEndPointService
  {
    #region Instance Variables
    private readonly string _azUrl = Environment.GetEnvironmentVariable("AzDvoUrl");
    private readonly string _apiEndpoint = Environment.GetEnvironmentVariable("AzDvoApiEndpoint");
    private readonly string _apiVersion = Environment.GetEnvironmentVariable("AzDvoApiVersion");
    private readonly string _personalAccessToken = Environment.GetEnvironmentVariable("AzDvoPersonalAccessToken");
    private readonly string _apiVersionPreview = Environment.GetEnvironmentVariable("AzDvoApiVersionPreview");

    IGlobalMapper<ServiceEndpoint, ServiceEndpointDto> _serviceEndpointMapper;
    private HttpClient _httpClient;
    #endregion
    #region Properties
    private string BaseUrl {get {return $"{_azUrl}{{0}}/{_apiEndpoint}";}}
    private string CheckEndpointRequestUrl
    {
      get{return $"serviceendpoint/endpoints?endpointNames={{0}}&{_apiVersionPreview}";}
    }
    private string CreateEndpointRequestUrl
    {
      get {return $"serviceendpoint/endpoints{_apiVersionPreview}";}
    }
    #endregion
    #region Constructor
    public EndPointService(IGlobalMapper<ServiceEndpoint, ServiceEndpointDto> serviceEndpointMapper)
    {
      _serviceEndpointMapper = serviceEndpointMapper; 
      _httpClient = new HttpClient();

      _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
      Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken))));
    }
    #endregion
    #region CheckIfServiceEndPointExist
    public async Task<ServiceEndpointDto> CheckIfServiceEndpointExists(string endpointName, string projectName)
    {
      if (string.IsNullOrEmpty(endpointName))
      {
        return new ServiceEndpointDto() { Error = new ErrorDto() { Message = "'serviceEndpointName' cannot be empty", Type = "CheckIfServiceEndpointExists" } };
      }
      if (string.IsNullOrEmpty(projectName))
      {
        return new ServiceEndpointDto() { Error = new ErrorDto() { Message = "'prjectName' cannot be empty", Type = "CheckIfServiceEndpointExists" } };
      }

      string endpoint = $"{string.Format(BaseUrl, projectName)}{string.Format(CheckEndpointRequestUrl, endpointName)}";

      HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var serviceEndpoints = JsonConvert.DeserializeObject<ServiceEndpointList>(responseContent);

        if (serviceEndpoints.Count == "0")
        {
          return null;
        }
        else if (serviceEndpoints.Count == "1")
        {
          var serviceEndpoint = _serviceEndpointMapper.Map(serviceEndpoints.Value[0]);
          return serviceEndpoint;
        }
        else
        {
          var serviceEndpoint = new ServiceEndpointDto()
          {
            Error = new ErrorDto()
            {
              Message = "Multiple service endpoints were found for this request. These need to be consolidated to one.",
              Type = "CheckIfServiceEndpointExists"
            }
          };

          return serviceEndpoint;
        }
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

        return new ServiceEndpointDto()
        {
          Error = new ErrorDto()
          {
            Message = message,
            Type = "CheckIfServiceEndpointExists"
          }
        };
      }
    }
   #endregion
    #region CreateServiceEndpoint
    public async Task<ServiceEndpointDto> CreateServiceEndpoint(string endpointName, string projectName, string Url, string type="git")
    {
      if (string.IsNullOrEmpty(endpointName))
      {
        return new ServiceEndpointDto() { Error = new ErrorDto() { Message = "'source repo Name' cannot be empty", Type = "CreateServiceEndpoint" } };
      }
      if (string.IsNullOrEmpty(projectName))
      {
        return new ServiceEndpointDto() { Error = new ErrorDto() { Message = "'projectName' cannot be empty", Type = "CreateServiceEndpoint" } };
      }
      if (string.IsNullOrEmpty(Url))
      {
        return new ServiceEndpointDto() { Error = new ErrorDto() { Message = "'gitUrl' cannot be empty", Type = "CreateServiceEndpoint" } };
      }
      
      string endpoint = $"{string.Format(BaseUrl, projectName)}{CreateEndpointRequestUrl}";
      
      ServiceEndpoint serviceEndpoint = new ServiceEndpoint()
      {
        Name = endpointName,
        // Type = "git",
        Type = type,
        Url = Url,
        Authorization = new ServiceEndpointAuthorization()
        { 
          Parameters = new ServiceEndpointParameters() { Username = _personalAccessToken },
          Scheme = "UsernamePassword"
        }
      };

      HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(endpoint, serviceEndpoint);

      if (responseMessage.IsSuccessStatusCode)
      {
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var serviceEndpointResponse = JsonConvert.DeserializeObject<ServiceEndpoint>(responseContent);
        var serviceEndpointDto = _serviceEndpointMapper.Map(serviceEndpointResponse);

        return serviceEndpointDto;
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

        return new ServiceEndpointDto()
        {
          Error = new ErrorDto()
          {
            Message = message,
            Type = "CreateServiceEndpoint"
          }
        };
      }

    }
    #endregion
  }
}