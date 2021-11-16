using Newtonsoft.Json;

namespace DevOps.TaskMaster.Orchestrator.Shared.Models
{
    public class PostResponse  {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("buildName")]
        public string BuildName { get; set; }
        [JsonProperty("buildDefinitionId")]
        public string BuildDefinitionId { get; set; } 
        [JsonProperty("releaseName")]
        public string ReleaseName { get; set; } 
        [JsonProperty("releaseDefinitionId")]
        public string ReleaseDefinitionId { get; set; } 
        [JsonProperty("status")]
        public string Status { get; set; } 
        [JsonProperty("error")]
        public Error Error { get; set; } 
    }
}