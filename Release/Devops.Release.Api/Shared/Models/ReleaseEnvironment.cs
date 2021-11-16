using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class ReleaseEnvironment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
}