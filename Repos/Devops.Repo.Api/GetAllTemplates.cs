using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevOps.Repo.Api.Shared.Services;

namespace DevOps.Repo.Api
{
  public class GetTemplatesFunc
 {
    #region Function instances
    ITemplateService _templateService;
    #endregion

    #region Constructor
    public GetTemplatesFunc(ITemplateService repoService)
    {
      _templateService = repoService;
    }
    #endregion

    #region Azure Function
    [FunctionName("GetAllTemplates")]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = "templates")] HttpRequest request,
      ILogger log)
    {
      log.LogInformation("GetAlltemplates Service Bus is trigger...");

      var templates = await _templateService.GetAllTemplates();

      string responseMessage = JsonConvert.SerializeObject(templates);
      return new OkObjectResult(responseMessage);
    }
    #endregion
  }
}
