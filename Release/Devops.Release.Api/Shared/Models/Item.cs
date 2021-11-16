using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class Item
    {
        public string ObjectId { get; set; }
        public bool IsFolder { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
    }
}