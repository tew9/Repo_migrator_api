using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevOps.Repo.Client
{
  public interface IRepositoryService
  {
    Task<HttpResponseMessage> CreateRepoWithTemplate(Object obj);
    Task<HttpResponseMessage> GetRepository(string projectName, string repoName);
    Task<HttpResponseMessage> DeleteRepository(string projectName, string repoName);
    Task<HttpResponseMessage> GetRepositories(string projectName);
    Task<HttpResponseMessage> CreateEmptyRepo(Object obj);
    Task<HttpResponseMessage> MigrateGitRepoItems(Object obj);
    Task<HttpResponseMessage> MigrateTfvcRepoItems(Object obj);
    Task<HttpResponseMessage> GetTfvcRepos(string projectName);
  }
}