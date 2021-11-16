namespace DevOps.TaskMaster.Orchestrator.Shared.Models
{
  public class GetResponse
  {
    public object Repo { get; set; }
    public object Release { get; set; }
    public object Build { get; set; }
    public object Error { get; set; }
  }
}