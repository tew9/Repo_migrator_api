using System.Threading.Tasks;
using DevOps.Repo.Contracts;

namespace DevOps.Repo.Api.Shared.Services
{
 public interface IProjectService
 {
   public Task<ProjectDto> GetProject(string projectName);
 } 
}