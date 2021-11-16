using System.Collections.Generic;
using System.Threading.Tasks;
using DevOps.Repo.Contracts;
using DevOps.Repo.Api.Shared.Models.Git;

namespace DevOps.Repo.Api.Shared.Services
{
  public interface IRepoService
  { 
    public Task<RepoDto> CreateRepo(string appName, ProjectDto project, string templateRepoName);
    public Task<RepoDto> CreateRepo(string repoName, ProjectDto project);
    public Task<ImportRequestDto> ImportRepo(string repoId, ServiceEndpointDto serviceEndpoint, string projectName, 
                                             bool deleteServiceEndpointAfterImportIsDone);
    public Task<string> GetImportRepoStatus(string repoId, string importRequestId, string projectName);
    public Task<List<RepoPushDto>> UpdateRepoFilesAndFolders(string repoId, string applicationName, string projectName);
    public Task<Repository> GetRepository(string projectName, string repoName);
    public Task<RepoList> GetRepositories(string projectName);
    public Task<RepoDeleteDto> DeleteRepo(RepoDto repo);
    public Task<Dictionary<string, string>> TemplateIntegration(string gitRemoteUrl, string[] extensions, string _workingdir, string committer, 
                                                                bool isCleaningUpAllowed, bool isJustIntegration);
  }
}