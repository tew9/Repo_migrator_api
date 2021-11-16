using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevOps.Release.Api.Shared.Models;

namespace DevOps.Release.Api.Shared.Models
{
    public class BuildDefinitionReference
    {
        // Lower case property names used. Capitalizing the first letter was not recognized by the Vsts Api as valid Json
        public BuildDefinition definition { get; set; }
        public Project project { get; set; }
        public Repository repository { get; set; }
        public DefaultVersionType defaultVersionType { get; set; }
    }
}