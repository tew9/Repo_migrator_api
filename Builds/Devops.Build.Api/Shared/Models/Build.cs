using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Build.Api.Shared.Models
{
    public class Builds
    {
        public string Id { get; set; }
        public BuildDefinition Definition { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
    }
}