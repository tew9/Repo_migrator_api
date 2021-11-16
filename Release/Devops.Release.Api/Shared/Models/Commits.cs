using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class Commit
    {
        public string Comment { get; set; }
        public List<Change> Changes { get; set; }
    }
}