using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevOps.Repo.Contracts
{
    public class CreateEmptyRepoRequestDto
    {
        [JsonProperty("destinationRepoName")]
        public string DestinationRepoName { get; set; }
        [JsonProperty("destinationProjectName")]
        public string DestinationProjectName { get; set; }
    }
}
