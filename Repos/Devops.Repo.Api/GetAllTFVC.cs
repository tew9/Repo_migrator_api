using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevOps.Repo.Contracts;
using DevOps.Repo.Api.Shared.Services;

namespace DevOps.Repo.Api
{
    public class GetAllTFVCFolders
    {
        private ITfvcRepoService _tfvcRepoService;

        public GetAllTFVCFolders(ITfvcRepoService tfvcRepoService)
        {
            _tfvcRepoService = tfvcRepoService;
        }

        [FunctionName("GetAllTfvcFolders")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "tfvc/{projectName}")] HttpRequest req, string projectName,
            ILogger log)
        {
            log.LogInformation("GetTFVC folders  Service Bus is triggered ...");

            if (string.IsNullOrEmpty(projectName))
            {
                var folder = new TfvcFolderDto () { Error = new ErrorDto() { Message = "Please, provide the projectname in your params", Type="GetTfvcFolders"}};
                return new BadRequestObjectResult(JsonConvert.SerializeObject(folder));
            }
            var folders = await _tfvcRepoService.GetAllFolders(projectName);
            return new OkObjectResult(JsonConvert.SerializeObject(folders));
        }
    }
}
