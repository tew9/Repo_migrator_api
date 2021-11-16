using DevOps.Build.Contracts;
using DevOps.Build.Api.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DevOps.Build.Api.Shared.Services
{
    public interface IBuildService
    {
        Task<BuildDefinitionDto> CreateBuildDefinition(string repoId, string applicationName, string projectName, string buildAgentName, string templateBuildName);
        Task<BuildDefinitionDeleteDto> DeleteBuildDefinition(string buildDefinitionId, string projectName);
        Task<BuildDefinitionDto> GetBuildDefinition(string buildDefinitionId, string projectName);
        Task<BuildDefinitionList> GetAllBuildDefinition(string projectName);
        //Task<BuildDto> QueueBuild(string buildDefinitionId, string projectName);
    }
}
