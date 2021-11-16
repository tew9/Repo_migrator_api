using DevOps.Release.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class ReleaseDefinitionList
    {
        public string Count { get; set; }
        public List<ReleaseDefinition> Value { get; set; }
        public ErrorDto Error { get; set; }

    }
}