using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Build.Api.Shared.Models
{
    public class Repo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProjectId { get; set; }
        public object Type { get; set; }
        public Properties Properties { get; set; }
        public object Error { get; set; }
        public Project Project { get; set; }
    }
}