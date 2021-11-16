using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevOps.Build.Contracts;

namespace DevOps.Build.Api.Shared.Models
{
    public class BuildDefinitionList
    {
        public string Count { get; set; }
        public List<BuildDefinitionDto> Value { get; set; }
       public ErrorDto Error { get; set; }
    }
}