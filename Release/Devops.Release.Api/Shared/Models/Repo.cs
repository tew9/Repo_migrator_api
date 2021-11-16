using DevOps.Release.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class Repository
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ProjectDto Project { get; set; }
        public string Type { get; set; }
        public PropertiesDto Properties { get; set; }
    }
}