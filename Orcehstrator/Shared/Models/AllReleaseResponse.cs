
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DevOps.TaskMaster.Orchestrator.Shared.Models
{
    public class ReleaseDefinition
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("Error")]
        public Error Error { get; set; }
    }
    public class ReleaseDefinitionList
    {
        public string Count { get; set; }
        public List<ReleaseDefinition> Value { get; set; }
       public Error Error { get; set; }
    }
}