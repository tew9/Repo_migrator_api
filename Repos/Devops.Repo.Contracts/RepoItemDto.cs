
using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
    public class RepoItemDto
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("IsFolder")]
        public bool IsFolder { get; set; }
        [JsonProperty("Path")]
        public string Path { get; set; }
    }
}