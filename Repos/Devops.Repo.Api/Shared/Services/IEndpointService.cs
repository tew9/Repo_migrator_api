using System.Threading.Tasks;
using DevOps.Repo.Contracts;

namespace DevOps.Repo.Api.Shared.Services
{
  public interface IEndPointService
  {
    public Task<ServiceEndpointDto> CheckIfServiceEndpointExists(string endpointName, string projectName);
    public Task<ServiceEndpointDto> CreateServiceEndpoint(string endpointName, string projectName, string Url, string type="git");
  }
}