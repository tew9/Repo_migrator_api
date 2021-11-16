using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevOps.Release.Api.Shared.Models
{
    public class Change
    {
        public string ChangeType { get; set; }
        public string SourceServerItem { get; set; }
        public Item Item { get; set; }
        public ItemContent NewContent { get; set; }
    }
}