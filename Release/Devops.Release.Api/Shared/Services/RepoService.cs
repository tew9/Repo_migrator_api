using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using DevOps.Release.Api.Shared.Mappers;
using DevOps.Release.Api.Shared.Models;
using DevOps.Release.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DevOps.Release.Api.Shared.Services
{
    public class RepoService : IRepoService
    {
        #region Service Instances
        private readonly string _azUrl = Environment.GetEnvironmentVariable("AzDvoUrl");
        private readonly string _apiEndpoint = Environment.GetEnvironmentVariable("AzDvoApiEndpoint");
        private readonly string _apiVersion = Environment.GetEnvironmentVariable("AzDvoApiVersion");
        private readonly string _personalAccessToken = Environment.GetEnvironmentVariable("AzDvoPersonalAccessToken");
        private readonly string _apiVersionPreview = Environment.GetEnvironmentVariable("AzDvoApiVersionPreview");
        HttpResponseMessage responseMessage;

        private IGlobalMapper<Repository, RepoDto> _repoMapper;
        private HttpClient _httpClient;
        #endregion
        #region Properties
        private string BaseUrl { get { return $"{_azUrl}{{0}}/{_apiEndpoint}"; } }
        private string GetRepoRequestUrl
        {
            get { return $"git/repositories/{{0}}?api-version=6.0"; }
        }
        
        #endregion

        #region Constructors
        public RepoService(IGlobalMapper<Repository, RepoDto> repoMapper)
        {
            _repoMapper = repoMapper;
            _httpClient = new HttpClient();
            
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken))));
        }
        #endregion
        #region GetOneRepo
        public async Task<RepoDto> GetRepository(string repoName, string projectName)
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

            responseMessage = await _httpClient.GetAsync(endpoint);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var repo = JsonConvert.DeserializeObject<Repository>(responseContent);
                var repoDto = _repoMapper.Map(repo);
                return repoDto;
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
                    message = "No clear explannation, but here's what I got " + e.Message;
                }

                return new RepoDto()
                {
                    Error = new ErrorDto()
                    {
                        Message = message,
                        Type = "GetRepo"
                    }
                };
            }
        }
        #endregion 
    }
}
