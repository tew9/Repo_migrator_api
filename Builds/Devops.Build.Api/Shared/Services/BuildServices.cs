using DevOps.Build.Contracts;
using Newtonsoft.Json;
using DevOps.Build.Api.Shared.Models;
using DevOps.Build.Api.Shared.Mappers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net;

namespace DevOps.Build.Api.Shared.Services
{

    public class BuildService : IBuildService
    {
        private HttpClient _httpClient;
        private IMapper<BuildDefinition, BuildDefinitionDto> _buildDefinitionMapper;
        private IMapper<BuildDefinition, BuildDefinitionList> _buildDefinitionListMapper;
        private IMapper<Builds, BuildDto> _buildMapper;
        
        HttpResponseMessage responseMessage;

        private readonly string _azUrl = Environment.GetEnvironmentVariable("AzDvoUrl");
        private readonly string _apiEndpoint = Environment.GetEnvironmentVariable("AzDvoApiEndpoint");
        private readonly string _apiVersion = Environment.GetEnvironmentVariable("AzDvoApiVersion");
        private readonly string _personalAccessToken = Environment.GetEnvironmentVariable("AzDvoPersonalAccessToken");
        private readonly string _apiVersionPreview = Environment.GetEnvironmentVariable("AzDvoApiVersionPreview");
        private readonly string _yamlBuildType = Environment.GetEnvironmentVariable("AzDvoApiYmlBuildType");
        private readonly string _gitRepoType = Environment.GetEnvironmentVariable("AzDvoApiYmlRepoType");
        private readonly string _yamlFilename = Environment.GetEnvironmentVariable("AzDvoApiYmlFileName");
        private readonly string _revision = Environment.GetEnvironmentVariable("BuilDefRevision");
        public BuildService(IMapper<BuildDefinition, BuildDefinitionDto> buildDefinitonMapper, IMapper<Builds, BuildDto> buildMapper, IMapper<BuildDefinition, BuildDefinitionList> buildDefinitionListMapper)
        {
            _buildDefinitionMapper = buildDefinitonMapper;
            _buildDefinitionListMapper = buildDefinitionListMapper;
            _buildMapper = buildMapper;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken))));
        }
        public async Task<BuildDefinitionDto> CreateBuildDefinition(string repoId, string applicationName, string projectName, string buildAgentName,
          string templateBuildName)
        {
            if (string.IsNullOrEmpty(repoId))
            {
                return new BuildDefinitionDto() { Error = new ErrorDto() { Message = "'repoId' cannot be empty", Type = "CreateBuildDefinition" } };
            }
            if (string.IsNullOrEmpty(applicationName))
            {
                return new BuildDefinitionDto() { Error = new ErrorDto() { Message = "'applicationName' cannot be empty", Type = "CreateBuildDefinition" } };
            }
            if (string.IsNullOrEmpty(projectName))
            {
                return new BuildDefinitionDto() { Error = new ErrorDto() { Message = "'projectName' cannot be empty", Type = "CreateBuildDefinition" } };
            }
            if (string.IsNullOrEmpty(buildAgentName))
            {
                return new BuildDefinitionDto() { Error = new ErrorDto() { Message = "'buildAgentName' cannot be empty", Type = "CreateBuildDefinition" } };
            }
            if (string.IsNullOrEmpty(templateBuildName))
            {
                return new BuildDefinitionDto() { Error = new ErrorDto() { Message = "'templateBuildName' cannot be empty", Type = "CreateBuildDefinition" } };
            }

            string buildName = templateBuildName.Replace("UniqueNameGoesHere", applicationName);
            buildName = buildName.Replace("uniquenamegoeshere", applicationName.ToLower());

            var buildDefinition = new BuildDefinition()
            {
                Name = buildName,
                Revision=Convert.ToInt32(_revision),
                Repository = new Repo()
                {
                    Id = repoId,
                    Type = _gitRepoType,
                    Properties = new Properties() { FullName = applicationName }
                },
                Process = new BuildProcess()
                {
                    Type = Convert.ToInt32(_yamlBuildType),
                    YamlFilename = _yamlFilename
                },
            };

            string endpoint = _azUrl+projectName+"/"+_apiEndpoint+"build/definitions"+_apiVersion;

            //string endpoint = "https://dev.azure.com/chkenergy/CHK/_apis/build/definitions?api-version=4.1";
            
            responseMessage = await _httpClient.PostAsJsonAsync(endpoint, buildDefinition);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var buildDefinitionResponse = JsonConvert.DeserializeObject<BuildDefinition>(responseContent);
                var buildDefinitionDto = await _buildDefinitionMapper.Map(buildDefinitionResponse);

                return  buildDefinitionDto;
            }
            else
            {
                string message;

                try
                {
                    message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
                }
                catch (Exception e)
                {
                    message = "No error message provided";
                }

                return new BuildDefinitionDto()
                {
                    Error = new ErrorDto()
                    {
                        Message = message,
                        Type = "CreateBuildDefinition"
                    }
                };
            }
        }

        public async Task<BuildDefinitionDeleteDto> DeleteBuildDefinition(string buildDefinitionId, string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                return new BuildDefinitionDeleteDto()
                {
                    Error = new ErrorDto()
                    {
                        Message = "'projectName' cannot be empty",
                        Type = "DeleteBuildDefinition"
                    }
                };
            }
     
            var buildDefinition = await GetBuildDefinition(buildDefinitionId, projectName);
            string endpoint = _azUrl + projectName + "/" + _apiEndpoint + "build/definitions/" + buildDefinition.Id + _apiVersion;

            HttpResponseMessage responseMessage = await _httpClient.DeleteAsync(endpoint);

            if (responseMessage.IsSuccessStatusCode)
            {
                return new BuildDefinitionDeleteDto() { Name = buildDefinition.Name, Deleted = true };
            }
            else
            {
                string message;

                try
                {
                    message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
                }
                catch (Exception e)
                {
                    message = "No error message provided";
                }

                return new BuildDefinitionDeleteDto() { Error = new ErrorDto() { Message = message, Type = "DeleteBuildDefinition" } };
            }
        }

        public async Task<BuildDefinitionDto> GetBuildDefinition(string definitionId, string projectName)
        {
            if (string.IsNullOrEmpty(definitionId))
            {
                return new BuildDefinitionDto() { Error = new ErrorDto() { Message = "'buildId' cannot be empty", Type = "GetBuild" } };
            }
            if (string.IsNullOrEmpty(projectName))
            {
                return new BuildDefinitionDto() { Error = new ErrorDto() { Message = "'projectName' cannot be empty", Type = "GetBuild" } };
            }
            string endpoint = _azUrl+projectName+"/"+_apiEndpoint+"build/definitions/"+definitionId+_apiVersion;
            HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var build = JsonConvert.DeserializeObject<BuildDefinition>(responseContent);
                var buildDto = await _buildDefinitionMapper.Map(build);

                return buildDto;
            }
            else
            {
                string message;

                try
                {
                    message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
                }
                catch (Exception e)
                {
                    message = "No error message provided";
                }

                return new BuildDefinitionDto()
                {
                    Error = new ErrorDto()
                    {
                        Message = message,
                        Type = "GetBuildDefinition"
                    }
                };
            }
        }

        public async Task<BuildDefinitionList> GetAllBuildDefinition(string projectName)
        {

            if (string.IsNullOrEmpty(projectName))
            {
                return new BuildDefinitionList() { Error = new ErrorDto() { Message = "'projectName' cannot be empty", Type = "GetAllBuildDefinitions" } };
            }
            string endpoint = _azUrl + projectName + "/" + _apiEndpoint + "build/definitions/";
            HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var buildDefinitions = JsonConvert.DeserializeObject<BuildDefinitionList>(responseContent);


                return buildDefinitions;
            }
            else
            {
                string message;
                string status;

                try
                {
                    message = responseMessage.Content.ReadAsAsync<HttpError>().Result.Message;
                    status = responseMessage.StatusCode.ToString();
                }
                catch (Exception e)
                {
                    message = "No error message provided";
                    status = responseMessage.StatusCode.ToString();
                }

                return new BuildDefinitionList()
                {
                    Error = new ErrorDto()
                    {
                        Message = message,
                        Status = status,
                        Type = "GetBuildDefinition"
                    }
                };
            }
        }
    }
}
