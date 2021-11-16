using Devops.Release.Client;
using DevOps.Build.Client;
using Newtonsoft.Json;
using DevOps.TaskMaster.Orchestrator.Shared.Models;
using System.Threading.Tasks;

namespace DevOps.TaskMaster.Orchestrator.Shared.Utilities
{
    public class Helper
    {
        private readonly IBuildService _BuildService;
        private readonly IReleaseService _ReleaseService;
        public Helper(IBuildService buildService, IReleaseService relaeseService)
        {
            this._BuildService = buildService;
            this._ReleaseService = relaeseService;
        }

        public async Task<BuildDefinition> CreateBuildDefinition(string projectName, string repoName, string repoId, string buildAgentName, string templateBuildName)
        {

           CreateBuildRequest buildRequest = new CreateBuildRequest()
           {
                buildName = repoName,
                projectName = projectName,
                repoId = repoId,
                buildAgentName = buildAgentName,
                templateBuildName = templateBuildName
           };

            var buildResponse = await _BuildService.CreateBuildDefinition(buildRequest);
            if (buildResponse.IsSuccessStatusCode)
            {
                var responseJson = await buildResponse.Content.ReadAsStringAsync();
                var buildStringResult = JsonConvert.DeserializeObject(responseJson).ToString();
                var buildResult = JsonConvert.DeserializeObject<BuildDefinition>(buildStringResult);

                return buildResult;
            }
            else
            {
                var responseJson = await buildResponse.Content.ReadAsStringAsync();
                var buildStringResult = JsonConvert.DeserializeObject(responseJson).ToString();
                var buildError = JsonConvert.DeserializeObject<BuildDefinition>(buildStringResult);

                BuildDefinition failedBuild = new BuildDefinition()
                {
                    Error = new Error { Message = buildError.Error.Message, Type = "build" },
                };

                return failedBuild;
            }
        }

        public async Task<ReleaseDefinition> CreateReleaseDefinition(string projectName,  string buildName, string repoName, string templateName)
        {
            CreateReleaseRequestDto releaseRequest = new CreateReleaseRequestDto()
            {
                releaseProject = projectName,
                buildName = buildName,
                repoName = repoName,
                templateName = templateName
            };

            var releaseResponse = await _ReleaseService.CreateReleaseDefinition(releaseRequest);
            if (releaseResponse.IsSuccessStatusCode)
            {
                var releaseRespose = await releaseResponse.Content.ReadAsStringAsync();
                var releasString = JsonConvert.DeserializeObject(releaseRespose).ToString();
                var releaseResult = JsonConvert.DeserializeObject<ReleaseDefinition>(releasString);

                return releaseResult;
            }
            else
            {
                dynamic releasString = null;
                var releaseRespose = await releaseResponse.Content.ReadAsStringAsync();
                releasString = JsonConvert.DeserializeObject(releaseRespose).ToString();
                ReleaseDefinition failedRelease = new ReleaseDefinition()
                {
                    Error = new Error { Message = JsonConvert.SerializeObject(releasString), Type = "Release" },
                };
                return failedRelease;
            }
            
        }
    }
}