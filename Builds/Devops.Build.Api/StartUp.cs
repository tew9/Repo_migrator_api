using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DevOps.Build.Api.Shared.Models;
using DevOps.Build.Contracts;
using DevOps.Build.Api.Shared.Mappers;
using DevOps.Build.Api.Shared.Services;

[assembly: FunctionsStartup(typeof(DevOps.Build.Api.Startup))]
namespace DevOps.Build.Api
{
    public class Startup : IWebJobsStartup
    {
        private readonly string _tenant = "cc16da7d-1b13-44cb-9c4f-4aa5421228b7";
        public void Configure(IWebJobsBuilder builder)
        { 
            builder.Services.AddScoped<IBuildService, BuildService>();
             builder.Services.AddScoped<IMapper<BuildDefinition, BuildDefinitionDto>, BuildDefinitionMapper>();
            builder.Services.AddScoped<IMapper<Builds, BuildDto>, BuildMapper>();
         
        }
    }
}
