using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
    public class RepoPushDto
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Error")]
        public ErrorDto Error { get; set; }
    }
}