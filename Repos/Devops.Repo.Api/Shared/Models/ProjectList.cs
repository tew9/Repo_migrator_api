using System.Collections.Generic;

namespace DevOps.Repo.Api.Shared.Models
{
  public class ProjectList
  {
    public string Count { get; set; }
    public List<Project> Value { get; set; }
  }
}