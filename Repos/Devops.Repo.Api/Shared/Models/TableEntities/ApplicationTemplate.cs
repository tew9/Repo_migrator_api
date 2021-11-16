using System;
using System.Threading.Tasks;
using DevOps.Repo.Contracts;
using Microsoft.WindowsAzure.Storage.Table;

namespace DevOps.Repo.Api.Shared.Models.TableEntities
{
  public class ApplicationTemplate: TableEntity
  {
    public ApplicationTemplate(string templateName)
    {
      this.PartitionKey = "Template";
      this.RowKey = templateName;
    }

    public ApplicationTemplate(){}

    public string BuildAgentName { get; set; }
    public string Name { get; set; }
    public string GitUrl { get; set; }
    public string Platform { get; set; }
    public string ReleaseDefinitionId { get; set; }
    public string BuildName { get; set; }
    public string ReleaseName { get; set; }
    public string RepoName { get; set; }
    public string RepoId { get; set; }
    public ErrorDto Error {get; set;}

    public static implicit operator Task<object>(ApplicationTemplate v)
    {
      throw new NotImplementedException();
    }
  }
    
}