using Newtonsoft.Json;

namespace DevOps.Build.Api.Shared.Models
{ 
    public class BuildRequest
    {
        [JsonProperty("buildName")]
        public string BuildName { get; set; }
        [JsonProperty("projectName")]
        public string ProjectName { get; set; }
        [JsonProperty("repoId")]
        public string RepoId { get; set; }
        [JsonProperty("buildAgentName")]
        public string BuildAgentName { get; set; }
        [JsonProperty("templateBuildName")]
        public string TemplateBuildName { get; set; }
    }
    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public object Error { get; set; }
    }
}
