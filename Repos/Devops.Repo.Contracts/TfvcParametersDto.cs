using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
  public class TfvcParametersDto
  { 
    [JsonProperty("tfvcSource")]
    public TfvcSourceDto TfvcSource { get; set; }
    [JsonProperty("deleteServiceEndpointAfterImportIsDone")]
    public bool DeleteServiceEndpointAfterImportIsDone { get; set; }
  } 

  public class TfvcSourceDto
  {
    public string path { get; set; }
    public bool importHistory { get; set; }
    public int importHistoryDurationInDays { get; set; }
  }
}