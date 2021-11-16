
using DevOps.Repo.Contracts;

namespace DevOps.Repo.Api.Shared.Models
{
  public class Project
  {
    public string Id { get; set; }
    public string Url { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string State { get; set; }
    public string LastUpdatTime { get; set; }
    public ErrorDto Error { get; set; }
  }
}