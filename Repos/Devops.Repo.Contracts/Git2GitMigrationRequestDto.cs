using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevOps.Repo.Contracts
{
    public class Git2GitMigrateRequestDto
    {
        [JsonProperty("destinationProjectName")]
        public string DestinationProjectName { get; set; }
        [JsonProperty("destinationRepoName")]
        public string DestinationRepoName { get; set; }
        [JsonProperty("gitSourceRepoName")]
        public string GitSourceRepoName { get; set; }
        [JsonProperty("sourceProjectName")]
        public string SourceProjectName { get; set; }
        [JsonProperty("repoType")]
        public string RepoType { get; set; } = "git";
        [JsonProperty("deleteServiceEndpointAfterImportIsDone")]
        public bool DeleteServiceEndpointAfterImportIsDone { get; set; } = true;
    }
}
