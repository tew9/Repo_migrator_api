using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class ServiceEndpointAuthorization
    {
        public ServiceEndpointParameters Parameters { get; set; }
        public string Scheme { get; set; }
    }
}