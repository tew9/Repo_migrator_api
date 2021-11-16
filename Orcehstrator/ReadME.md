# Introduction 
This is the repository for the **taskmaster Orchestrator** which consume three microservices to create, migrate, get, or delete repos with their associated builds or release.  

NB: ***build or release is not migrated with the repo when one is migrating either tfs repos to git or git repo to another git repo, instead, anew build and release pipeline is created based on the standardized template [here](https://dev.azure.com/chkenergy/CHK-DevOpsExamples/_git/YAMLBuildTemplate).***

---

### CI/CD
Continuous Integration Pipeline | [DevOps_TaskMaster_Orchestrator](https://dev.azure.com/chkenergy/CHK-Iterative/_build?definitionId=1305)

Continuous Delivery Releases | [DevOps_TaskMaster_Orchestrator](https://dev.azure.com/chkenergy/CHK-Iterative/_release?_a=releases&view=mine&definitionId=328)

This repo is meant to be built and released.  

**Subject Experts**:
- Peter Adam
- Sean Peak
- Josh Hawthorn
- Tango Tew
---

### Tools
Built in ```Visual Studio 2019/ Visual Studio Code ```

Uses ```Aliencube.AzureFunctions.Extensions.OpenApi --version 3.1.1 ```

Uses ```.Net Core version 3.1``` [here](https://dotnet.microsoft.com/download/dotnet-core/3.1)

Uses ```Azure Function 3.1 ```

Uses ```TaskMaster.CommonClient``` nugget package [here](https://dev.azure.com/chkenergy/CHK-DevOpsExamples/_packaging?_a=package&feed=CHK-NuGet&package=TaskMaster.Common.Client&protocolType=NuGet&version=1.0.0)

Uses ```TaskMaster.Build.Client``` nugget package [here](https://dev.azure.com/chkenergy/CHK-DevOpsExamples/_packaging?_a=package&feed=CHK-NuGet&package=DevOps.Build.Client&protocolType=NuGet&version=1.2.4) to access the build Microservice.

Uses ```TaskMaster.Repo.Client``` nugget package [here](https://dev.azure.com/chkenergy/CHK-DevOpsExamples/_packaging?_a=package&feed=CHK-NuGet&package=DevOps.Repo.Client&protocolType=NuGet&version=1.0.9)  to access the repos Microservice..

Uses ```TaskMaster.Repo.Client``` nugget package [here](https://dev.azure.com/chkenergy/CHK-DevOpsExamples/_packaging?_a=package&feed=CHK-NuGet&package=DevOps.Release.Client&protocolType=NuGet&version=1.2.6)  to access the release Microservice..

**Note**: Always keep gatewayURl property of CommonClient filled with the right appgatwayUrl or else the orchestrator will not be authenticated by the clients even if one has correct oktaclientId and ClientSecrete

---

## Related Notes
This Orchestrator, is the consumer of the three microservices which makes up taskmaster app migrator. To successfuly run this orchestrator one will only need to clone the repo and run it, and test it through openApi already built into the function app.  

## 1. Running Orchestrator locally
1. Clone and open DevOps_Tasmaster_Repos repo in your local.

2. All the tasks are being performed in their respective functions located in ./DevOps.Taskmaster.Orchestrator

3. Open Azure Function CLI running the app and open swaggerUI link in you browser or copy the link to the browser, and enjoy UI consumption of your microservices.


**Enjoy coding.** 

## 2. Implementaion

***1. The MicroServices/functions Are Consumed from Api gateway in this Function App as Follows:***

1. download the Repo.Client, Build.Client, Release.Client and CommonClient nugget from DevOps package source. 
Provide your okta(security information) to the nugget through dependency injection as following example in your startUp.

```C#
    var config = new ClientConfig()
    {
        ApplicationName = "Orchestrator",
        OktaClientId = "OktaClientId",
        OktaClientSecret = "OktaClientSecret",
        OktaTokenUrl = "OktaTokenUrl",
        DevOpsApiBaseUrl = "ApiGatewayUrl"
    };

    builder.Services.AddScoped<IRepositoryService, RepositoryService>((s) => { return new RepositoryService(config); });
    builder.Services.AddScoped<ITemplateService, TemplateService>((s) => { return new TemplateService(config); });
            
    builder.Services.AddScoped<IReleaseService, ReleaseService>((s) => { return new ReleaseService(config); });
           
    builder.Services.AddScoped<IBuildService, BuildService>((s) => { return new BuildService(config); });

```
Now you can just inject those services in your constructor where ever you want to use them.

The nugget package will feed those oath2 security props to the services and you'll be granted an access to the microservices service.

***For more information on the payload for orchestrator, I'd advice to use the openApi UI on this orchestrator by running it locally, so you know what to expect.***

**Note On Utilizing the microservices from APIM.**  
In your Postman, One will need to always have a token granted from correct environment ontaclientID and Secrete.

**Enjoy migration**