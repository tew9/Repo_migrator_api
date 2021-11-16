using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
    public class ImportRequestDto
    {
        [JsonProperty("Id")]
        public string ImportRequestId { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
        [JsonProperty("Error")]
        public ErrorDto Error { get; set; }
        [JsonProperty("Parameters")]
        public ParametersDto Parameters { get; set; }
    }

    public class GitImportTFVCSourceDto
    {
        [JsonProperty("Id")]
        public string ImportRequestId { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
        [JsonProperty("Error")]
        public ErrorDto Error { get; set; }
        [JsonProperty("Parameters")]
        public TfvcParametersDto Parameters { get; set; }
        [JsonProperty("RepoId")]
        public string RepoId { get; set; } //Target Repo
    }

    public class ParametersDto
    { 
        public GitSourceDto GitSource { get; set; }
        public string ServiceEndpointId { get; set; }   
        public bool DeleteServiceEndpointAfterImportIsDone { get; set; }
    }

    public class GitSourceDto
    {
        public string Url { get; set; }
    }
}