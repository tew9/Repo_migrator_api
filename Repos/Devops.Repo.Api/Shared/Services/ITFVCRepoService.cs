using System.Threading.Tasks;
using DevOps.Repo.Contracts;
using DevOps.Repo.Api.Shared.Models.Git;

namespace DevOps.Repo.Api.Shared.Services
{
  public interface ITfvcRepoService
  { 
    public Task<GitImportTFVCSourceDto> TfvcImportRequest(TfvcParametersDto param, string repoId, string projectName);
    public Task<TfvcFolderDto> GetTfvcFolder(string folderPath, string projectName);
    public Task<TfvcFolderList> GetAllFolders(string projectName);
  }
}