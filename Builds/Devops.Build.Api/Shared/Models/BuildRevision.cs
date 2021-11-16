using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Build.Api.Shared.Models
{
    public class BuildRevision
    {
        public string Revision { get; set; }
        public string Name { get; set; }
        public string ChangedDate { get; set; }
    }
}