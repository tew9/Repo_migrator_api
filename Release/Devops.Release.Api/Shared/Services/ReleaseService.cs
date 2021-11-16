using DevOps.Release.Api.Shared.Mappers;
using DevOps.Release.Api.Shared.Models;
using DevOps.Release.Api.Shared.TableEntities;
using DevOps.Release.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace DevOps.Release.Api.Shared.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly string _azUrl = Environment.GetEnvironmentVariable("AzDvoUrl");
        private readonly string _apiEndpoint = Environment.GetEnvironmentVariable("AzDvoApiEndpoint");
        private readonly string _apiVersion = Environment.GetEnvironmentVariable("AzDvoApiVersion");
        private readonly string _personalAccessToken = Environment.GetEnvironmentVariable("AzDvoPersonalAccessToken");
        private readonly string _apiVersionPreview = Environment.GetEnvironmentVariable("AzDvoApiVersionPreview");
        private readonly string _yamlBuildType = Environment.GetEnvironmentVariable("AzDvoApiYmlBuildType");
        private readonly string _gitRepoType = Environment.GetEnvironmentVariable("AzDvoApiYmlRepoType");
        private readonly string _yamlFilename = Environment.GetEnvironmentVariable("AzDvoApiYmlFileName");
        private readonly string _devopsexamples = Environment.GetEnvironmentVariable("AzDvoDevOpsExamples");
        private readonly string _devopsexamplesId = Environment.GetEnvironmentVariable("AzDvoDevOpsExamplesId");
        private readonly string _azUrlVsrm = Environment.GetEnvironmentVariable("AzDvoApiUrlVsrm");


        private IMapper<BuildDefinition, BuildDefinitionDto> _buildDefinitionMapper;
        private HttpClient _httpClient;
        private IMapper<ServiceEndpoint, ServiceEndpointDto> _serviceEndpointMapper;
        private IMapper<ReleaseDefinition, ReleaseDefinitionDto> _releaseDefinitionMapper;



        public ReleaseService(IMapper<ReleaseDefinition, ReleaseDefinitionDto> releaseDefinitionMapper,
          ITemplateService templateService, IMapper<BuildDefinition, BuildDefinitionDto> buildDefinitonMapper,
          IGlobalMapper<ApplicationTemplate, ApplicationTemplateDto> templateMapper,
          IMapper<ServiceEndpoint, ServiceEndpointDto> serviceEndpointMapper)
        {
            _serviceEndpointMapper = serviceEndpointMapper;
            _buildDefinitionMapper = buildDefinitonMapper;
            _releaseDefinitionMapper = releaseDefinitionMapper;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken))));
        }
        public async Task<BuildDefinitionDto> GetBuildDefinition(string definitionName, string projectName)
        {
            if (string.IsNullOrEmpty(definitionName))
            {
                return new BuildDefinitionDto() { Error = new ErrorDto() { Message = "'buildDefinition Name' cannot be empty", Type = "GetBuildDefinition" } };
            }
            if (string.IsNullOrEmpty(projectName))
            {
                return new BuildDefinitionDto() { Error = new ErrorDto() { Message = "'projectName' cannot be empty", Type = "GetBuildDefinition" } };
            }

            string endpoint = _azUrl + projectName + "/" + _apiEndpoint + "build/definitions" + _apiVersion;
            HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                dynamic jsonBody = JObject.Parse(responseContent);

                if (jsonBody["value"] != null && ((JArray)jsonBody["value"]).HasValues)
                {
                    List<BuildDefinition> definitions = JsonConvert.DeserializeObject<List<BuildDefinition>>(jsonBody["value"].ToString());
                    var buildDefinition = definitions.Where(d => d.Name == definitionName).FirstOrDefault();
                    BuildDefinitionDto buildDefDto = await _buildDefinitionMapper.Map(buildDefinition);
                    if (buildDefDto == null)
                    {
                        return new BuildDefinitionDto() { Error = new ErrorDto() { Message = $"No release exists with name '{definitionName}' Please check the releaseName and the releaseProject to be correct", Type = "GetBuildDefinition" } };
                    }
                    return buildDefDto;
                }
                return null;
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

        public async Task<ReleaseDefinitionList> GetReleaseDefinitions(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                return new ReleaseDefinitionList() { Error = new ErrorDto() { Message = "'projectName' cannot be empty", Type = "GetReleaseDefinitions" } };
            }

            string endpoint = _azUrlVsrm + projectName + "/" + _apiEndpoint + "release/definitions"+_apiVersion;
            HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var definitions = JsonConvert.DeserializeObject<ReleaseDefinitionList>(responseContent);
                return definitions;
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

                return new ReleaseDefinitionList()
                {
                    Error = new ErrorDto()
                    {
                        Message = message,
                        Type = "GetReleaseDefinitions"
                    }
                };
            }

        }

        public async Task<ReleaseDefinitionDto> CreateReleaseDefinition(string repoId, ApplicationDto application, string buildDefinitionId, ApplicationTemplateDto template)
        {
            if (string.IsNullOrEmpty(repoId))
            {
                return new ReleaseDefinitionDto() { Error = new ErrorDto() { Message = "'repoId' cannot be empty", Type = "CreateReleaseDefinition" } };
            }
            if (application == null)
            {
                return new ReleaseDefinitionDto() { Error = new ErrorDto() { Message = "'application' cannot be null", Type = "CreateReleaseDefinition" } };
            }
            if (string.IsNullOrEmpty(buildDefinitionId))
            {
                return new ReleaseDefinitionDto() { Error = new ErrorDto() { Message = "'buildDefinitionId' cannot be empty", Type = "CreateReleaseDefinition" } };
            }
            if (template == null)
            {
                return new ReleaseDefinitionDto() { Error = new ErrorDto() { Message = "'template' cannot be empty", Type = "CreateReleaseDefinition" } };
            }
            var sourceProj = template.GitUrl.Split("/")[3];

            var releaseTemplate = await GetReleaseDefintionTemplate(sourceProj, template.ReleaseDefinitionId);

            if (releaseTemplate == null)
            {
                return new ReleaseDefinitionDto()
                {
                    Error = new ErrorDto()
                    {
                        Message = "Failed to retreive release template JSON content",
                        Type = "GetReleaseDefinitionTemplate"
                    }
                };
            }

            string releaseDefinitionJson = await ModifyReleaseDefinitionTemplate(repoId, application.Name, application.Project.Name, application.Project.Id, application.BuildDefinition.Id, template.BuildAgentName, releaseTemplate,
             template.BuildName, template.ReleaseName, template.Platform);
            if (string.IsNullOrEmpty(releaseDefinitionJson))
            {
                return new ReleaseDefinitionDto()
                {
                    Error = new ErrorDto()
                    {
                        Message = "Failed to update release template JSON content",
                        Type = "ModifyReleaseDefinitionTemplate"
                    }
                };
            }

            var stringContent = new StringContent(releaseDefinitionJson, UnicodeEncoding.UTF8, "application/json");

            string endpoint = _azUrlVsrm + application.Project.Name + "/" + _apiEndpoint + "release/definitions" + _apiVersion;

            HttpResponseMessage responseMessage = await _httpClient.PostAsync(endpoint, stringContent);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var releaseDefinition = JsonConvert.DeserializeObject<ReleaseDefinition>(responseContent);
                var releaseDefinitionDto = await _releaseDefinitionMapper.Map(releaseDefinition);

                return releaseDefinitionDto;
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

                return new ReleaseDefinitionDto()
                {
                    Error = new ErrorDto()
                    {
                        Message = message,
                        Type = "CreateReleaseDefinition"
                    }
                };
            }
        }

        public async Task<dynamic> GetReleaseDefintionTemplate(string projectName, string releaseDefinitionId)
        {

            if (string.IsNullOrEmpty(releaseDefinitionId))
            {
                return null;
            }

            string endpoint = _azUrlVsrm + projectName + "/" + _apiEndpoint + "release/definitions/" + releaseDefinitionId + _apiVersion;

            HttpResponseMessage responseMessage = await _httpClient.GetAsync(endpoint);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                dynamic release = JsonConvert.DeserializeObject(responseContent);
                return release;
            }

            return null;
        }

        public async Task<string> ModifyReleaseDefinitionTemplate(string repoId, string Name, string projectName, string projectId, string buildDefinitionId, string buildAgent, dynamic releaseTemplate,
             string templateBuildName, string templateReleaseName, string templatePlatform)
        {
            if (string.IsNullOrEmpty(repoId) || string.IsNullOrEmpty(buildDefinitionId) || releaseTemplate == null)
            {
                return null;
            }

            string releaseTemplateArtifactAlias = null;
            string newArtifactAlias = "_" + templateBuildName.Replace("UniqueNameGoesHere", Name);
            newArtifactAlias = newArtifactAlias.Replace("uniquenamegoeshere", Name);

            try
            {
                string releaseName = templateReleaseName.Replace("UniqueNameGoesHere", Name);
                releaseName = releaseName.Replace("uniquenamegoeshere", Name);

                releaseTemplate.Name = releaseName;

                // Can add this back in later if determined necessary. Sets the AppCI variable, but fails if AppCI variable doesn't exist.
                // releaseTemplate.variables.AppCI.value = application.Name.ToLower();

                releaseTemplateArtifactAlias = releaseTemplate.artifacts[0].alias;
            }
            catch (Exception)
            {
                return null;
            }

            var releaseArtifacts = new List<ReleaseArtifact>();

            var releaseArtifact = new ReleaseArtifact()
            {
                SourceId = projectId + ":" + buildDefinitionId,
                Type = "Build",
                Alias = newArtifactAlias,
                DefinitionReference = new BuildDefinitionReference()
                {
                    definition = new BuildDefinition() { Id = buildDefinitionId },
                    project = new Project() { Id = projectId },
                    repository = new Repository() { Id = repoId },
                    defaultVersionType = new DefaultVersionType()
                    {
                        Id = "latestType",
                        Name = "Latest"
                    }
                },
                IsPrimary = true,
                IsRetained = false
            };

            releaseArtifacts.Add(releaseArtifact);
            // we now have a release definition payload ready to create but we need to strip out IDs and anything
            // containing IDs from our "template"
            // since ADO will assign new IDs for that when we POST the contents we are manipulating here
            releaseTemplate.Remove("id");
            releaseTemplate.Remove("url");
            releaseTemplate.Remove("_links");
            string releaseTemplateJson = JsonConvert.SerializeObject(releaseTemplate);
            string releaseArtifactsJson = JsonConvert.SerializeObject(releaseArtifacts);

            // Find out where the artifacts array is in the template JSON
            string artifactsJsonProperty = "artifacts\":[{";
            int beginningIndexOfArtifactsProperty = releaseTemplateJson.IndexOf(artifactsJsonProperty);

            if (beginningIndexOfArtifactsProperty == -1)
            {
                return null;
            }

            int openBracketIndex = releaseTemplateJson.IndexOf("[", beginningIndexOfArtifactsProperty);

            if (openBracketIndex == -1)
            {
                return null;
            }

            int closeBracketIndex = releaseTemplateJson.IndexOf("]", openBracketIndex);

            if (closeBracketIndex == -1)
            {
                return null;
            }

            // Replace the template's source artifact with the newly created ReleaseArtifact
            releaseTemplateJson = releaseTemplateJson.Remove(openBracketIndex, closeBracketIndex - openBracketIndex + 1)
                .Insert(openBracketIndex, releaseArtifactsJson);

            // Find and replace UniqueNameGoesHere references and rename with the application name
            releaseTemplateJson = releaseTemplateJson.Replace(releaseTemplateArtifactAlias, newArtifactAlias);
            releaseTemplateJson = releaseTemplateJson.Replace("UniqueNameGoesHere", Name);
            releaseTemplateJson = releaseTemplateJson.Replace("uniquenamegoeshere", Name);

            // Find and replace the template's queueId(s) based on target project
            if (projectId != _devopsexamplesId)
            {
               // string targetAgentId = buildAgent.GetType().GetProperty(projectName.Replace("", "")).GetValue(buildAgent).ToString();
               string targetAgentId = "8"; //buildAgent.ToString();
                if (string.IsNullOrEmpty(targetAgentId))
                {
                    return null;
                }

                string queueIdJsonProperty = "queueId\":";
                int countOfQueueId = (releaseTemplateJson.Length - releaseTemplateJson.Replace(queueIdJsonProperty, "").Length) / queueIdJsonProperty.Length;
                int startingPosition = 0;

                for (int i = 1; i <= countOfQueueId; i++)
                {
                    int beginningIndexOfQueueIdProperty = releaseTemplateJson.IndexOf(queueIdJsonProperty, startingPosition);

                    if (beginningIndexOfQueueIdProperty == -1)
                    {
                        return null;
                    }

                    int semicolonBeforeQueueIdValue = releaseTemplateJson.IndexOf(":", beginningIndexOfQueueIdProperty);

                    if (semicolonBeforeQueueIdValue == -1)
                    {
                        return null;
                    }

                    int commaAfterQueueIdValue = releaseTemplateJson.IndexOf(",", semicolonBeforeQueueIdValue);

                    if (commaAfterQueueIdValue == -1)
                    {
                        return null;
                    }

                    releaseTemplateJson = releaseTemplateJson.Remove(semicolonBeforeQueueIdValue + 1, commaAfterQueueIdValue - semicolonBeforeQueueIdValue - 1)
                        .Insert(semicolonBeforeQueueIdValue + 1, targetAgentId);

                    startingPosition = commaAfterQueueIdValue;
                }
            }

            // Find and replace any Azure Subscription service connections in the release tasks if target project is not CHK-DevOpsExamples
            if (templatePlatform.ToLower() == "azure" && projectId != _devopsexamplesId)
            {
                string targetchkapplicationbuildid = await GetAzureSubscriptionServiceConnectionId("CHK Application Build", projectName);
                string templatechkapplicationbuildid = await GetAzureSubscriptionServiceConnectionId("CHK Application Build", _devopsexamples);
                string targetchkapplicationrunid = await GetAzureSubscriptionServiceConnectionId("CHK Application Build", projectName);
                string templatechkapplicationrunid = await GetAzureSubscriptionServiceConnectionId("CHK Application Build", _devopsexamples);

                releaseTemplateJson = releaseTemplateJson.Replace(templatechkapplicationbuildid, targetchkapplicationbuildid);
                releaseTemplateJson = releaseTemplateJson.Replace(templatechkapplicationrunid, targetchkapplicationrunid);
            }

            return releaseTemplateJson;
        }

        public async Task<string> GetAzureSubscriptionServiceConnectionId(string serviceEndpointName, string projectName)
        {
            if (string.IsNullOrEmpty(serviceEndpointName) || string.IsNullOrEmpty(projectName))
            {
                return null;
            }

            string endpoint = _azUrl + projectName + "/" + _apiEndpoint + "serviceendpoint/endpoints" + "?type=azurerm&endpointnames=" + serviceEndpointName + _apiVersion;


            // string endpoint = @"https://dev.azure.com/chkenergy/CHK/_apis/serviceendpoint/endpoints?type=azurerm&endpointnames=serviceEndpointName&api-version=6.0-preview.4";
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
                    var serviceEndpoint = await _serviceEndpointMapper.Map(serviceEndpoints.Value[0]);

                    return serviceEndpoint.Id;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }

        public async Task<ReleaseDefinitionDeleteDto> DeleteReleaseDefinition(string releaseDefinitionId, string releaseDefinitionName, string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                return new ReleaseDefinitionDeleteDto()
                {
                    Error = new ErrorDto()
                    {
                        Message = "'projectName' cannot be empty",
                        Type = "DeleteReleaseDefinition"
                    }
                };
            }


            string endpoint = _azUrlVsrm + projectName + "/" + _apiEndpoint + "release/definitions/" + releaseDefinitionId + _apiVersion;

            //  DELETE https://vsrm.dev.azure.com/{organization}/{project}/_apis/release/definitions/{definitionId}?api-version=6.1-preview.4

            HttpResponseMessage responseMessage = await _httpClient.DeleteAsync(endpoint);

            if (responseMessage.IsSuccessStatusCode)
            {
                return new ReleaseDefinitionDeleteDto() { Name = releaseDefinitionName, Deleted = true };
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

                return new ReleaseDefinitionDeleteDto() { Error = new ErrorDto() { Message = message, Type = "DeleteReleaseDefinition" } };
            }
        }
    }
}
