using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevOps.Repo.Contracts
{
    public class CreateRepoFromTemplateRequestDto
    {

        [JsonProperty("destinationRepoName")]
        public string DestinationRepoName { get; set; }
        [JsonProperty("templateName")]
        public string TemplateName { get; set; }
        [JsonProperty("destinationProjectName")]
        public string DestinationProjectName { get; set; }
        [JsonProperty("integration")]
        public bool Integration { get; set; } = false;
        [JsonProperty("cleanUp")]
        public bool CleanUp { get; set; } = false;
        [JsonProperty("committerEmail")]
        public string? Email { get; set; }
    }
}
