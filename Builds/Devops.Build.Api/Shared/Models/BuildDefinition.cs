using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevOps.Build.Api.Shared.Models;


namespace DevOps.Build.Api.Shared.Models
{
    public class BuildDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Revision { get; set; }
        public string Path { get; set; }

        public Repo Repository { get; set; }
        public BuildProcess Process { get; set; }
        public BuildQueue Queue { get; set; }
    }
}