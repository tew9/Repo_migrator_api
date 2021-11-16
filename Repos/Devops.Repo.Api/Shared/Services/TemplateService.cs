using System.Net.Mime;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;
using DevOps.Repo.Api.Shared.Mappers;
using DevOps.Repo.Contracts;
using DevOps.Repo.Api.Shared.Models.TableEntities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace DevOps.Repo.Api.Shared.Services
{
  public class TemplateService: ITemplateService
  {
    #region Instance Variables & Constants 
    private string _templateMetadataTable = Environment.GetEnvironmentVariable("AzureStorageAccountTableTemplateMetadataName");
    private string connectionString = Environment.GetEnvironmentVariable("AzureStorageConnection");

    IGlobalMapper<ApplicationTemplate, ApplicationTemplateDto> _mapper;
    CloudTableClient _tableClient;
    CloudStorageAccount _storageAccount;
    CloudTable _table;
    #endregion

    #region Constructors
    public TemplateService(IGlobalMapper<ApplicationTemplate, ApplicationTemplateDto> globalMapper)
    {
      _mapper = globalMapper;
      _storageAccount = CloudStorageAccount.Parse(connectionString);
      _tableClient = _storageAccount.CreateCloudTableClient();
      _table = _tableClient.GetTableReference(_templateMetadataTable);
    }
    #endregion

    #region GetOneTemplate
    public async Task<ApplicationTemplate> GetTemplate(string templateName)
    {
      if (string.IsNullOrEmpty(templateName))
      {
        return new ApplicationTemplate () { Error = new ErrorDto() { Message = $"please provide the templateName/RowKey value", Type = "GetOneTemplate" } };
      }

      TableOperation retrieveOperation = TableOperation.Retrieve<ApplicationTemplate>("Template", templateName);
      TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

      if (retrievedResult.Result == null)
      {
         return new ApplicationTemplate () { Error = new ErrorDto() { Message = $"No template in the table with RowKey of '{templateName}'", Type = "GetOneTemplate" } };
      }
      
      var applicationTemplate = (ApplicationTemplate)retrievedResult.Result;
      //var applicationTemplateDto = _mapper.Map(applicationTemplate);

      return applicationTemplate;
    }
    #endregion
    #region GetAllTemplates
    public async Task<List<ApplicationTemplate>> GetAllTemplates()
    {
      TableQuery<ApplicationTemplate> query = new TableQuery<ApplicationTemplate>()
      .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Template"));  
      var templates = new List<ApplicationTemplate>();

      var retrievedResult =  await _table.ExecuteQuerySegmentedAsync(query, null);
     
      if (retrievedResult.Results.Count <= 0)
      {
        templates.Add(new ApplicationTemplate() { Error = new ErrorDto() { Message = "No template in the table with PartitionKey of 'Template'", Type = "Get All Template" } });
        return templates;
      }
      
      foreach(var item in retrievedResult)
      {
        templates.Add(item);
      };
      
      return templates;
    }
    #endregion
  
  }
}