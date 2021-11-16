using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DevOps.Release.Api.Shared.Services;
using DevOps.Release.Api.Shared.Mappers;
using DevOps.Release.Api.Shared.Models;
using DevOps.Release.Contracts;
using DevOps.Release.Api.Shared.TableEntities;

[assembly: FunctionsStartup(typeof(DevOps.Release.Api.Startup))]
namespace DevOps.Release.Api
{
    public class Startup : IWebJobsStartup
    {
        private readonly string _tenant = "cc16da7d-1b13-44cb-9c4f-4aa5421228b7";
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddScoped<IReleaseService, ReleaseService>();
            builder.Services.AddScoped<IGlobalMapper<ApplicationTemplate, ApplicationTemplateDto>,
                                 GlobalMapper<ApplicationTemplate, ApplicationTemplateDto>>();
            builder.Services.AddScoped<IGlobalMapper<Repository, RepoDto>,
                                 GlobalMapper<Repository, RepoDto>>();
            builder.Services.AddScoped<IMapper<ReleaseDefinition, ReleaseDefinitionDto>, ReleaseDefinitionMapper>();
            builder.Services.AddScoped<IMapper<BuildDefinition, BuildDefinitionDto>, BuildDefinitionMapper>();
            builder.Services.AddScoped<IMapper<Releases, ReleaseDto>, ReleaseMapper>();
            builder.Services.AddScoped<IMapper<ServiceEndpoint, ServiceEndpointDto>, ServiceEndpointMapper>();
            builder.Services.AddScoped<ITemplateService, TemplateService>();
            builder.Services.AddScoped<IRepoService, RepoService>();
        }
    }
}


