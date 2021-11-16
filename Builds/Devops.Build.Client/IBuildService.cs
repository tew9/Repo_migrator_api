using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevOps.Build.Client
{
  public interface IBuildService
  {
    Task<HttpResponseMessage> CreateBuildDefinition(object obj);
    Task<HttpResponseMessage> DeleteBuildDefinition(string buildDefinitionId, string projectName);
    Task<HttpResponseMessage> GetBuildDefinition(string buildDefinitionId, string projectName);
    Task<HttpResponseMessage> GetAllBuildDefinition(string projectName);
  }
}
