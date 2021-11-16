using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
  public class ApplicationDeleteDto
  {
    [JsonProperty("Repo")]
    public RepoDeleteDto Repo { get; set; }
    public ReleaseDefinitionDeleteDto Release { get; set; }
    [JsonProperty("Error")]
    public ErrorDto Error { get; set; }
  }
}