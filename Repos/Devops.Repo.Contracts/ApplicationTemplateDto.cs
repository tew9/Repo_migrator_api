
using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
    public class ApplicationTemplateDto
    {
        [JsonProperty("RowKey")]
        public string RowKey { get; set; }
        [JsonProperty("BuildAgentName")]
        public string BuildAgentName { get; set; }
        [JsonProperty("GitUrl")]
        public string GitUrl { get; set; }
        [JsonProperty("Platform")]
        public string Platform { get; set; }
        [JsonProperty("ReleaseDefinitionId")]
        public string ReleaseDefinitionId { get; set; }
        [JsonProperty("BuildName")]
        public string BuildName { get; set; }
        [JsonProperty("ReleaseName")]
        public string ReleaseName { get; set; }
        [JsonProperty("RepoName")]
        public string RepoName { get; set; }
        [JsonProperty("RepoId")]
        public string RepoId { get; set; }
        public ErrorDto Error {get; set;}
    }
}
