
using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
  public class ProjectDto
  {
    [JsonProperty("Id")]
    public string Id { get; set; }
    [JsonProperty("State")]
    public string State { get; set; }
    [JsonProperty("Description")]
    public string Description { get; set; }
    [JsonProperty("LastUpdateTime")]
    public string LastUpdateTime { get; set; }
    [JsonProperty("Name")]
    public string Name { get; set; }
    [JsonProperty("Url")]
    public string Url { get; set; }
    [JsonProperty("Error")]
    public ErrorDto Error { get; set; }
  }
}