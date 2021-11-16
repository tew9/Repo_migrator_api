using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
    public class ApplicationRequestDto
    {
        [JsonProperty("RequestId")]
        public string RequestId { get; set; }
        [JsonProperty("AppType")]
        public string AppType { get; set; }
    }
}