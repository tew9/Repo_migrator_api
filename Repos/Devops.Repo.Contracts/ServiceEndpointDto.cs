using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
    public class ServiceEndpointDto
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Url")]
        public string Url { get; set; }
        [JsonProperty("Type")]
        public string Type { get; set; }
        [JsonProperty("Error")]
        public ErrorDto Error { get; set; }
    }
}