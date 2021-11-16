using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevOps.Repo.Contracts
{

    public class Tfvc2GitMigrateRequestDto
    {
        [JsonProperty("destinationProjectName")]
        public string DestinationProjectName { get; set; }
        [JsonProperty("destinationRepoName")]
        public string DestinationRepoName { get; set; }
        [JsonProperty("sourceProjectName")]
        public string SourceProjectName { get; set; }
        [JsonProperty("repoType")]
        public string RepoType { get; set; } = "tfvc";
        [JsonProperty("deleteServiceEndpointAfterImportIsDone")]
        public bool DeleteServiceEndpointAfterImportIsDone { get; set; } = true;
        [JsonProperty("tfvcSource")]
        public TfvcSourceDto TfvcSource { get; set; }
    }
}
