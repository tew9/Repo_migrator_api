using DevOps.Release.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevOps.Release.Api.Shared.Services
{
    public interface IRepoService
    {
        public Task<RepoDto> GetRepository(string repoName, string projectName);
    }
}