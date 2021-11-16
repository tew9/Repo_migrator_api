using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class ServiceEndpointList
    {
        public string Count { get; set; }
        public List<ServiceEndpoint> Value { get; set; }
    }
}