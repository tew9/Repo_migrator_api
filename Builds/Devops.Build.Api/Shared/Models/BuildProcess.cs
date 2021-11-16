using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Build.Api.Shared.Models
{
    public class BuildProcess
    {
        public int Type { get; set; }
        public string YamlFilename { get; set; }
    }
}