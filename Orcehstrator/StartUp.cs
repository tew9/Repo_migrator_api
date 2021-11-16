using DevOps.Build.Client;
using DevOps.Repo.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using TaskMaster.Common.Client;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Devops.Release.Client;

[assembly: FunctionsStartup(typeof(Orchestrator.Startup))]
namespace Orchestrator
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {

            var config = new ClientConfig()
            {
                ApplicationName = "Orchestrator",
                OktaClientId = Environment.GetEnvironmentVariable("OktaClientId"),
                OktaClientSecret = Environment.GetEnvironmentVariable("OktaClientSecret"),
                OktaTokenUrl = Environment.GetEnvironmentVariable("OktaTokenUrl"),
                DevOpsApiBaseUrl = Environment.GetEnvironmentVariable("ApiGatewayUrl")
            };

            builder.Services.AddScoped<IRepositoryService, RepositoryService>((s) => { return new RepositoryService(config); });
            builder.Services.AddScoped<ITemplateService, TemplateService>((s) => { return new TemplateService(config); });
            
            builder.Services.AddScoped<IReleaseService, ReleaseService>((s) => { return new ReleaseService(config); });
            builder.Services.AddScoped<IBuildService, BuildService>((s) => { return new BuildService(config); });
        }
    }
}
