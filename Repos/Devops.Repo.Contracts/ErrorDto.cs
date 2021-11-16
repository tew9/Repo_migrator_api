
using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
  public class ErrorDto
  {
    [JsonProperty("Message")]
    public string Message { get; set; }
    [JsonProperty("Type")]
    public string Type { get; set; }
  }
}