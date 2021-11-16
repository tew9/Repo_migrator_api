using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.Common.Client;

namespace DevOps.Repo.Client
{
    public class RepositoryService : IRepositoryService
    {
        private readonly JsonSerializerSettings _serializeSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        private readonly string _oktaClientId;
        private readonly string _oktaClientSecret;
        private readonly string _oktaTokenUrl;
        private readonly string _repoApiBaseUrl;
        private string _token;
        private readonly HttpClient _httpClient;

        public RepositoryService(IClientConfig config)
        {
            _oktaClientId = config.OktaClientId;
            _oktaClientSecret = config.OktaClientSecret;
            _oktaTokenUrl = config.OktaTokenUrl;
            _repoApiBaseUrl = config.DevOpsApiBaseUrl;
            _httpClient = new HttpClient { BaseAddress = new Uri(_repoApiBaseUrl) };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> CreateRepoWithTemplate(object obj)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(obj, _serializeSettings);
                var endpoint = $"{_repoApiBaseUrl}/taskmaster/repos/v1/repos/templates";
                var oktaToken = await GetOktaToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oktaToken);
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
                var response = await _httpClient.PostAsync(endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<HttpResponseMessage> CreateEmptyRepo(object obj)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(obj, _serializeSettings);
                var endpoint = $"{_repoApiBaseUrl}/taskmaster/repos/v1/repos";
                var oktaToken = await GetOktaToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oktaToken);
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
                var response = await _httpClient.PostAsync(endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<HttpResponseMessage> MigrateGitRepoItems(object obj)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(obj, _serializeSettings);
                var endpoint = $"{_repoApiBaseUrl}/taskmaster/repos/v1/repos/git";
                var oktaToken = await GetOktaToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oktaToken);
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
                var response = await _httpClient.PutAsync(endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<HttpResponseMessage> MigrateTfvcRepoItems(object obj)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(obj, _serializeSettings);
                var endpoint = $"{_repoApiBaseUrl}/taskmaster/repos/v1/repos/tfvc";
                var oktaToken = await GetOktaToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oktaToken);
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
                var response = await _httpClient.PutAsync(endpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<HttpResponseMessage> DeleteRepository(string projectName, string repoName)
        {
            try
            {
                var endpoint = $"{_repoApiBaseUrl}/taskmaster/repos/v1/repos/{projectName}/{repoName}";
                var oktaToken = await GetOktaToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oktaToken);
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
                var response = await _httpClient.DeleteAsync(endpoint);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<HttpResponseMessage> GetRepository(string projectName, string repoName)
        {
            try
            {
                var endpoint = $"{_repoApiBaseUrl}/taskmaster/repos/v1/repos/{projectName}/{repoName}";
                var oktaToken = await GetOktaToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oktaToken);
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
                var response = await _httpClient.GetAsync(endpoint);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<HttpResponseMessage> GetRepositories(string projectName)
        {
            try
            {
                var endpoint = $"{_repoApiBaseUrl}/taskmaster/repos/v1/repos/{projectName}";
                var oktaToken = await GetOktaToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oktaToken);
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
                var response = await _httpClient.GetAsync(endpoint);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<HttpResponseMessage> GetTfvcRepos(string projectName)
        {
            try
            {
                var endpoint = $"{_repoApiBaseUrl}/taskmaster/repos/v1/tfvc/{projectName}";
                var oktaToken = await GetOktaToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oktaToken);
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
                var response = await _httpClient.GetAsync(endpoint);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private async Task<string> GetOktaToken()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_token)) return _token;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                      Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_oktaClientId}:{_oktaClientSecret}")));

                List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("grant_type", "client_credentials"),
      };

                HttpRequestMessage request = new HttpRequestMessage { RequestUri = new Uri(_oktaTokenUrl), Method = HttpMethod.Post };
                request.Content = new FormUrlEncodedContent(requestData);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                string jsonResponse = await response.Content.ReadAsStringAsync();
                AccessToken accessToken = JsonConvert.DeserializeObject<AccessToken>(jsonResponse);
                _token = accessToken.access_token;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return _token;
        }

        class AccessToken
        {
            public string scope { get; set; }
            public string access_token { get; set; }
            public string token_type { get; set; }
            public long expires_in { get; set; }
        }

    }
}
