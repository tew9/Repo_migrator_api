using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using DevOps.Release.Api.Shared.TableEntities;
using DevOps.Release.Contracts;
using DevOps.Release.Api.Shared.Mappers;

namespace DevOps.Release.Api.Shared.Services
{
    public class TemplateService : ITemplateService
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
        public async Task<ApplicationTemplateDto> GetTemplate(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                return new ApplicationTemplateDto() { Error = new ErrorDto() { Message = $"please provide the templateName/RowKey value", Type = "GetOneTemplate" } };
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<ApplicationTemplate>("Template", templateName);
            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            if (retrievedResult.Result == null)
            {
                return new ApplicationTemplateDto() { Error = new ErrorDto() { Message = $"No template in the table with RowKey of '{templateName}'", Type = "GetOneTemplate" } };
            }

            var applicationTemplate = (ApplicationTemplate)retrievedResult.Result;
            var applicationTemplateDto = _mapper.Map(applicationTemplate);

            return applicationTemplateDto;
        }
        #endregion

    }
}