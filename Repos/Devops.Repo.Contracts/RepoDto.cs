using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
  public class RepoDto
  {
    [JsonProperty("Id")]
    public string Id { get; set; }
    [JsonProperty("Name")]
    public string Name { get; set; }
    [JsonProperty("size")]
    public long Size { get; set; }
    [JsonProperty("_links")]
    public LinksDto _links { get; set; }
    [JsonProperty("Project")]
    public ProjectDto Project { get; set; }
    [JsonProperty("Type")]
    public string Type { get; set; }
    [JsonProperty("Error")]
    public ErrorDto Error { get; set; }
  }
}