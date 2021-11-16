using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class ReleaseArtifact
    {
        public string SourceId { get; set; }
        public string Type { get; set; }
        public string Alias { get; set; }
        public BuildDefinitionReference DefinitionReference { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsRetained { get; set; }

    }
}