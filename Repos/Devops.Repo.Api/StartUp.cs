using DevOps.Repo.Api.Shared.Mappers;
using DevOps.Repo.Api.Shared.Services;
using DevOps.Repo.Contracts;
using DevOps.Repo.Api.Shared.Models.TableEntities;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using DevOps.Repo.Api.Shared.Models;
using DevOps.Repo.Api.Shared.Models.Git;
using DevOps.Repo.GitAutomation;
using Microsoft.Extensions.Options;
using Microsoft.Azure.WebJobs.Host.Bindings;

[assembly: FunctionsStartup(typeof(DevOps.Repo.Api.Startup))]
namespace DevOps.Repo.Api
{
    public class Startup : IWebJobsStartup
  {
    public void Configure(IWebJobsBuilder builder)
    {
      var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Project, ProjectDto>());
      builder.Services.AddScoped<IGlobalMapper<ApplicationTemplate, ApplicationTemplateDto>,
                                 GlobalMapper<ApplicationTemplate, ApplicationTemplateDto>>();
      builder.Services.AddScoped<IGlobalMapper<ServiceEndpoint, ServiceEndpointDto>, 
                                 GlobalMapper<ServiceEndpoint, ServiceEndpointDto>>();
      builder.Services.AddScoped<IGlobalMapper<Project, ProjectDto>, GlobalMapper<Project, ProjectDto>>();
      builder.Services.AddScoped<IGlobalMapper<Repository, RepoDto>, GlobalMapper<Repository, RepoDto>>();
      builder.Services.AddScoped<IGlobalMapper<Item, RepoItemDto>, GlobalMapper<Item, RepoItemDto>>();
      builder.Services.AddScoped<IGlobalMapper<PushResponse, RepoPushDto>, GlobalMapper<PushResponse, RepoPushDto>>();
      builder.Services.AddScoped<IGlobalMapper<ImportRequest, ImportRequestDto>, GlobalMapper<ImportRequest, ImportRequestDto>>();
      builder.Services.AddScoped<IGlobalMapper<TfvcFolder, TfvcFolderDto>, GlobalMapper<TfvcFolder, TfvcFolderDto>>();
      builder.Services.AddScoped<IGlobalMapper<GitImportTFVCSource, GitImportTFVCSourceDto>, 
                                 GlobalMapper<GitImportTFVCSource, GitImportTFVCSourceDto>>();

      builder.Services.AddScoped<ITemplateService, TemplateService>();
      builder.Services.AddScoped<IProjectService, ProjectService>();
      builder.Services.AddScoped<IRepoService, RepoService>();
      builder.Services.AddScoped<IEndPointService, EndPointService>();
      builder.Services.AddScoped<IFileService, FileService>();
      builder.Services.AddScoped<ITfvcRepoService, TfvcRepoService>();
      builder.Services.AddScoped<IGitAutomationService, GitAutomationService>();

      
      var executionContextOptions = builder.Services.BuildServiceProvider().GetService<IOptions<ExecutionContextOptions>>().Value;
      var appDirectory = executionContextOptions.AppDirectory;
    }
  }
}
