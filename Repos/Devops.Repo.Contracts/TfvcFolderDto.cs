using Newtonsoft.Json;

namespace DevOps.Repo.Contracts
{
  public class TfvcFolderDto
  {
    [JsonProperty("version")]
    public string Version { get; set; }
    [JsonProperty("changeDate")]
    public string ChangeDate { get; set; }
    [JsonProperty("contentMetadat")]
    public ContentMetaDataDto ContentMetaData { get; set; }
    [JsonProperty("_links")]
    public TfvcLinksDto _links { get; set; }
    [JsonProperty("path")]
    public string Path { get; set; }
    [JsonProperty("isFolder")]
    public bool isFolder { get; set; }
    [JsonProperty("Error")]
    public ErrorDto Error { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
  }

  public class ContentMetaDataDto
  {
    [JsonProperty("fileName")]
    public string FileName { get; set; }
  }
}