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
  public class GetTemplateFunc
  {
    #region Function Variables
    private ITemplateService _templateService;
    #endregion

    #region Constructor
    public GetTemplateFunc(ITemplateService templateService)
    {
      _templateService = templateService;
    }
    #endregion

    #region Function
    [FunctionName("GetOneTemplate")]
    public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "templates/{templateName}")] HttpRequest request, string templateName,
    ILogger log)
    {
      log.LogInformation("GetOneTemplate is triggered ...");
      string responseMessage;
      
      if(string.IsNullOrEmpty(templateName)){
        responseMessage = "This HTTP triggered function executed successfully, but temlate name/RowKey is Required\n";
        return new OkObjectResult(responseMessage);
      }
      
      #region SearchTemplate
      var template = await _templateService.GetTemplate(templateName);
      #endregion
      responseMessage = JsonConvert.SerializeObject(template);
      return new OkObjectResult(responseMessage);
    }
    #endregion
  }
}
