using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class BranchPolicy
    {
        public bool IsEnabled { get; set; }
        public bool IsBlocking { get; set; }
        public BranchPolicyType Type { get; set; }
        public dynamic Settings { get; set; }
    }
}