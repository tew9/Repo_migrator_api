using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class ImportRequest
    {
        public string ImportRequestId { get; set; }
        public Parameters Parameters { get; set; }
        public string Status { get; set; }
    }

    public class Parameters
    {
        public GitSource GitSource { get; set; }
        public string ServiceEndpointId { get; set; }
    }

    public class GitSource
    {
        public string Url { get; set; }
    }
}