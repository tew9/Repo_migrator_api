using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace DevOps.Release.Api.Shared.TableEntities
{
    public class ApplicationTemplate : TableEntity
    {
        public ApplicationTemplate(string templateName)
        {
            this.PartitionKey = "Template";
            this.RowKey = templateName;
        }

        public ApplicationTemplate() { }

        public string BuildAgentName { get; set; }
        public string GitUrl { get; set; }
        public string Platform { get; set; }
        public string ReleaseDefinitionId { get; set; }
        public string BuildName { get; set; }
        public string ReleaseName { get; set; }
        public string RepoName { get; set; }
        public string RepoId { get; set; }
    }
}