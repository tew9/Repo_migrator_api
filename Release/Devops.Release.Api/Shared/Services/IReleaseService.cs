using DevOps.Release.Api.Shared.Models;
using DevOps.Release.Contracts;
using System.Threading.Tasks;


namespace DevOps.Release.Api.Shared.Services
{
    public interface IReleaseService
    {
        Task<dynamic> GetReleaseDefintionTemplate(string projectName, string releaseDefinitionId);
        Task<ReleaseDefinitionList> GetReleaseDefinitions(string projectName);
        Task<string> ModifyReleaseDefinitionTemplate(string repoId, string Name, string projectName, string projectId, string buildDefinitionId, string buildAgent, dynamic releaseTemplate,
             string templateBuildName, string templateReleaseName, string templatePlatform);
        Task<ReleaseDefinitionDto> CreateReleaseDefinition(string repoId, ApplicationDto application, string buildDefinitionId, ApplicationTemplateDto template);
        Task<ReleaseDefinitionDeleteDto> DeleteReleaseDefinition(string releaseDefinitionId, string releaseDefinitionName, string projectName);
        Task<BuildDefinitionDto> GetBuildDefinition(string definitionName, string projectName);
        
    }
}
