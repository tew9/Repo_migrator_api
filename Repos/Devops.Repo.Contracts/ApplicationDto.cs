using System.Collections.Generic;
using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
  public class ApplicationDto
  {
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("committer")]
    public string Email { get; set; }
    [JsonProperty("cleanUp")]
    public bool CleanUp { get; set; }
    [JsonProperty("integration")]
    public bool Integration { get; set; }
    [JsonProperty("destinationRepoName")]
    public string DestinationRepoName { get; set; }
    [JsonProperty("gitSourceRepoName")]
    public string GitSourceRepoName { get; set; }
    [JsonProperty("Status")]
    public string Status { get; set; }
    [JsonProperty("Error")]
    public ErrorDto Error { get; set; }
    [JsonProperty("templateName")]
    public string TemplateName { get; set; }
    [JsonProperty("destinationProjectName")]
    public string DestinationProjectName { get; set; }
    [JsonProperty("sourceProjectName")]
    public string SourceProjectName { get; set; }
    [JsonProperty("Project")]
    public ProjectDto Project { get; set; }
    [JsonProperty("sourceProject")]
    public ProjectDto SourceProject { get; set; }
    [JsonProperty("Repo")]
    public RepoDto Repo { get; set; }
    [JsonProperty("sourceRepo")]
    public RepoDto SourceRepo { get; set; }
    [JsonProperty("ServiceEndpoint")]
    public ServiceEndpointDto ServiceEndpoint { get; set; }
    [JsonProperty("ImportRequest")]
    public ImportRequestDto ImportRequest { get; set; }
    [JsonProperty("TfvcImportRequest")]
    public GitImportTFVCSourceDto TfvcImportRequest { get; set; }
    [JsonProperty("RepoPushes")]
    public List<RepoPushDto> RepoPushes { get; set; }
    [JsonProperty("repoType")]
    public string RepoType { get; set; }
    [JsonProperty("Template")]
    public ApplicationTemplateDto Template { get; set;}
    [JsonProperty("tfvcSource")]
    public TfvcSourceDto TfvcSource { get; set; }
    [JsonProperty("deleteServiceEndpointAfterImportIsDone")]
    public bool DeleteServiceEndpointAfterImportIsDone { get; set; }
  }
}