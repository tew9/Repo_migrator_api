using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class RefUpdate
    {
        public string Name { get; set; }
        public string OldObjectId { get; set; }
        public string ObjectId { get; set; }
    }
}