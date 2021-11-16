using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using System.Net;
using DevOps.TaskMaster.Orchestrator.Shared.Models;
using DevOps.Repo.Client;
using DevOps.Build.Client;
using Devops.Release.Client;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Enums;
using DevOps.TaskMaster.Orchestrator.Shared.Utilities;

namespace DevOps.TaskMaster.Orchestrator
{
    public partial class Orchestrator
    {
        private readonly IRepositoryService _RepoService;
        private readonly IBuildService _BuildService;
        private readonly IReleaseService _ReleaseService;
        private readonly ITemplateService _templateService;

        public Orchestrator(IBuildService buildService, IReleaseService relaeseService, ITemplateService templateService, IRepositoryService repoService)
        {
            this._BuildService = buildService;
            this._ReleaseService = relaeseService;
            this._RepoService = repoService;
            this._templateService = templateService;
        }

        [FunctionName("CreateEmptyRepo")]
        [OpenApiOperation("createEmptyRepo","CreateEmptyRepo")]
        [OpenApiRequestBody("application/json", typeof(RepoCreateEmptyRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Repository), Summary = "The response", Description = "This returns the response")]
        public async Task<IActionResult> CreateRepo([HttpTrigger(AuthorizationLevel.Function, "Post", Route = "repos")]
         HttpRequest request, ILogger log)
        {
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            var requestJson = JsonConvert.DeserializeObject<RepoCreateEmptyRequest>(requestBody);
            var response = await _RepoService.CreateEmptyRepo(requestJson);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var repoStringResult = JsonConvert.DeserializeObject(responseContent).ToString();
                var repoResult = JsonConvert.DeserializeObject<Repository>(repoStringResult);

                if (repoResult.Error == null)
                {
                    return new OkObjectResult(repoResult);
                }
                else
                {
                    return new BadRequestObjectResult(repoResult);
                }
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var repoStringResult = JsonConvert.DeserializeObject(responseContent);
                return new BadRequestObjectResult(repoStringResult);
            }
        }

        //Migrate template and create build and release
        [FunctionName("RepoTransformOperation")]
        [OpenApiOperation(operationId: "createFromTemplate", tags: new [] { "RepoTransformOperation" }, Summary = "transform repo from the template", Description = "This Api will transform existing tfvc repo into git and create build and release pipeline for the new repo", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", bodyType: typeof(RepoCreateTemplateRequest), Description = "fill all the information requested to make a request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(PostResponse), Summary = "The response", Description = "This returns the response")]
        public async Task<IActionResult> CreateRepoWithTemplate([HttpTrigger(AuthorizationLevel.Function, "post", Route = "template")]
         HttpRequest request, ILogger log)
        {
            RepoResponse repoResult = null;
            PostResponse finalResult = null;
            BuildDefinition buildResult = null;
            bool repoSucceeded = false;
            bool buildSucceeded = false;
            Helper _helper = new Helper(_BuildService, _ReleaseService);

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            var requestJson = JsonConvert.DeserializeObject<RepoCreateTemplateRequest>(requestBody);
            var temp = await _RepoService.CreateRepoWithTemplate(requestJson);

            if (temp.IsSuccessStatusCode)
            {
                var responseContent = await temp.Content.ReadAsStringAsync();
                var repoStringResult = JsonConvert.DeserializeObject(responseContent).ToString();
                repoResult = JsonConvert.DeserializeObject<RepoResponse>(repoStringResult);

                finalResult = new PostResponse()
                {
                    Name = repoResult.Repo.Name,
                    Id = repoResult.Repo.Id,
                    Status = "Partally Completed"
                };
                repoSucceeded = true;
            }
            else
            {
                var responseContent = await temp.Content.ReadAsStringAsync();
                var repoStringResult = JsonConvert.DeserializeObject(responseContent).ToString();

                finalResult = new PostResponse()
                {
                    Error = new Error { Message = repoStringResult, Type = "repo" },
                    Status = "Failed"
                };
            }

            //create build definition
            if (requestJson.build && repoSucceeded)
            {

                buildResult = _helper.CreateBuildDefinition(repoResult.ProjectName, repoResult.Repo.Name, repoResult.Repo.Id, repoResult.Template.BuildAgentName, repoResult.Template.TemplateBuildName).Result;

                if (buildResult.Error == null)
                {
                    finalResult = new PostResponse()
                    {
                        Name = repoResult.Repo.Name,
                        Id = repoResult.Repo.Id,
                        BuildName = buildResult.Name,
                        BuildDefinitionId = buildResult.Id,
                        Status = "Partally Completed"
                    };
                    buildSucceeded = true;
                }
                else
                {
                    finalResult = new PostResponse()
                    {
                        Name = repoResult.Repo.Name,
                        Id = repoResult.Repo.Id,
                        Error = buildResult.Error,
                        Status = "Failed"
                    };
                }
            }

            //create release definition
            if (requestJson.release && buildSucceeded)
            {

                var releaseResult = _helper.CreateReleaseDefinition(repoResult.ProjectName, buildResult.Name, repoResult.Repo.Name, requestJson.templateName).Result;
                if (releaseResult.Error == null)
                {
                    finalResult = new PostResponse()
                    {
                        Name = repoResult.Repo.Name,
                        Id = repoResult.Repo.Id,
                        BuildName = buildResult.Name,
                        BuildDefinitionId = buildResult.Id,
                        ReleaseDefinitionId = releaseResult.Id,
                        ReleaseName = releaseResult.Name,
                        Status = "Fully Completed"
                    };
                }
                else
                {
                    finalResult = new PostResponse()
                    {
                        Name = repoResult.Repo.Name,
                        Id = repoResult.Repo.Id,
                        BuildName = buildResult.Name,
                        BuildDefinitionId = buildResult.Id,
                        Error = new Error { Message = releaseResult.Error.Message, Type = "release" },
                        Status = "failed"
                    };
                }
            }

            return new OkObjectResult(finalResult);

        }

        //Migrate Tfvc to git
        [FunctionName("Tfvc2GitMigration")]
        [OpenApiOperation(operationId: " tfvcToGit", tags: new [] { "TfvcGitmigration" }, Summary = "migrate tfvc repo", Description = "This Api will migrate tfvc changesets(items) to git repo in the same or another project", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(Tfvc2GitRequest), Description = "fill all the information requested to make a request")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RepoResponse), Summary = "The response", Description = "This returns the response")]
        public async Task<IActionResult> CreateAndMigrate([HttpTrigger(AuthorizationLevel.Function, "put", Route = "repos/tfvc")]
         HttpRequest request, ILogger log)
        {
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            var requestJson = JsonConvert.DeserializeObject<Tfvc2GitRequest>(requestBody);
            var temp = await _RepoService.CreateEmptyRepo(requestJson);
            if (temp.IsSuccessStatusCode)
            {
                var responseContent = await temp.Content.ReadAsStringAsync();
                var repoStringResult = JsonConvert.DeserializeObject(responseContent).ToString();
                var repoResult = JsonConvert.DeserializeObject<Repository>(repoStringResult);

                if (repoResult.Error == null)
                {
                    var migration = await _RepoService.MigrateTfvcRepoItems(requestJson);
                    if (migration.IsSuccessStatusCode)
                    {
                        var migrateResponse = await migration.Content.ReadAsStringAsync();
                        var migrateStringResult = JsonConvert.DeserializeObject(migrateResponse).ToString();
                        var migrateResult = JsonConvert.DeserializeObject<RepoResponse>(migrateStringResult);
                        return new OkObjectResult(JsonConvert.SerializeObject(migrateResult));
                    }
                    else
                    {
                        var migrateResponse = await migration.Content.ReadAsStringAsync();
                        var migrateStringResult = JsonConvert.DeserializeObject(migrateResponse);
                        return new BadRequestObjectResult(migrateStringResult);
                    }
                }
                else
                {
                    return new OkObjectResult(repoResult);
                }
            }
            else
            {
                var responseContent = await temp.Content.ReadAsStringAsync();
                var repoStringResult = JsonConvert.DeserializeObject(responseContent);
                return new BadRequestObjectResult(repoStringResult);
            }
        }

        //Migrate git to git
        [FunctionName("Git2GitMigration")]
        [OpenApiOperation(operationId: " gitToGit", tags: new[] { "Git2Gitmigration" }, Summary = "migrate git repo", Description = "This Api will migrate git repo to an empty git repo in the same or another project", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(Git2GitRequest), Description = "fill all the information requested to make a request")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RepoResponse), Summary = "The response", Description = "This returns the response")]
        public async Task<IActionResult> Git2GitMigration([HttpTrigger(AuthorizationLevel.Function, "put", Route = "repos/git")]
         HttpRequest request, ILogger log)
        {
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            var requestJson = JsonConvert.DeserializeObject<Git2GitRequest>(requestBody);
            var temp = await _RepoService.CreateEmptyRepo(requestJson);
            if (temp.IsSuccessStatusCode)
            {
                var responseContent = await temp.Content.ReadAsStringAsync();
                var repoStringResult = JsonConvert.DeserializeObject(responseContent).ToString();
                var repoResult = JsonConvert.DeserializeObject<Repository>(repoStringResult);

                if (repoResult.Error == null)
                {
                    var migration = await _RepoService.MigrateGitRepoItems(requestJson);
                    if (migration.IsSuccessStatusCode)
                    {
                        var migrateResponse = await migration.Content.ReadAsStringAsync();
                        var migrateStringResult = JsonConvert.DeserializeObject(migrateResponse).ToString();
                        var migrateResult = JsonConvert.DeserializeObject<RepoResponse>(migrateStringResult);
                        return new OkObjectResult(migrateResult);
                    }
                    else
                    {
                        var migrateResponse = await migration.Content.ReadAsStringAsync();
                        var migrateStringResult = JsonConvert.DeserializeObject(migrateResponse);
                        return new BadRequestObjectResult(migrateStringResult);
                    }
                }
                else
                {
                    return new BadRequestObjectResult(repoResult);
                }
            }
            else
            {
                var responseContent = await temp.Content.ReadAsStringAsync();
                var repoStringResult = JsonConvert.DeserializeObject(responseContent);
                return new BadRequestObjectResult(repoStringResult);
            }
        }
    }
}
