
using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
  public class RepoDeleteDto
  {
    [JsonProperty("Name")]
    public string Name { get; set; }
    [JsonProperty("Deleted")]
    public bool Deleted { get; set; }
    [JsonProperty("Error")]
    public ErrorDto Error { get; set; }
  }
}