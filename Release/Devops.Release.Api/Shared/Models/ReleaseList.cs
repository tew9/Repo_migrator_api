using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class Releases
    {
        public string Id { get; set; }
        public string DefinitionId { get; set; }
        public List<ReleaseEnvironment> Environments { get; set; }
    }
}