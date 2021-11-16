
namespace DevOps.Repo.Contracts
{
  public class LinksDto
  {
    public  PullRequestLink PullRequests { get; set; }
    public CommitLink Commits { get; set; }
    public ItemLink Items { get; set; }
    public PushLink Pushes { get; set; }
  }

  public class PullRequestLink
  {
    public  string Href { get; set; }
  }
  public class PushLink
  {
    public  string Href { get; set; }
  }
  public class CommitLink
  {
    public  string Href { get; set; }
  }

  public class ItemLink
  {
    public  string Href { get; set; }
  }
}