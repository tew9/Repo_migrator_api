using DevOps.Repo.Contracts;

namespace DevOps.Repo.Api.Shared.Models.Git
{
 public class TfvcParams
  {
    public TfvcSourceDto TfvcSource { get; set; }
    public bool DeleteServiceEndpointAfterImportIsDone { get; set; }
  } 
}