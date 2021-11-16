using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class ServiceEndpoint
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public ServiceEndpointAuthorization Authorization { get; set; }
    }
}