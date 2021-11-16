
namespace DevOps.TaskMaster.Orchestrator.Shared.Models
{
  public class DeletedResponse
  {
    public RepoDeleteResponse Repo { get; set; }
    public RepoDeleteResponse Release { get; set; }
    public RepoDeleteResponse Build { get; set; }
    public Error Error { get; set; }
  }

  public class RepoDeleteResponse
  {
    public string Name { get; set; }
    public bool Deleted { get; set; }
    public Error Error { get; set; }
  }
}