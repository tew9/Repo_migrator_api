using System;
using System.Collections.Generic;
using System.Text;

namespace DevOps.TaskMaster.Orchestrator.Shared.Models
{
    public class GetAllResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public string Url { get; set; }
        public string DefaultBranch { get; set; }
        public string RemoteUrl { get; set; }
        public Project Project { get; set; }
        public object Type { get; set; }
        public Links _links { get; set; }
        public object Error { get; set; }
    }
    //public class Repository
    //{
    //    public string Id { get; set; }
    //    public string Name { get; set; }
    //    public string Url { get; set; }
    //    public string DefaultBranch { get; set; }
    //    public string RemoteUrl { get; set; }
    //    public Project Project { get; set; }
    //    public string Type { get; set; }
    //    public Links _links { get; set; }
    //    public Error Error { get; set; }
    //}
    public class Project
    {
        public string Id { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public object Error { get; set; }
    }

    public class PullRequests
    {
        public string Href { get; set; }
    }

    public class Commits
    {
        public string Href { get; set; }
    }

    public class Items
    {
        public string Href { get; set; }
    }

    public class Pushes
    {
        public string Href { get; set; }
    }

    public class Links
    {
        public PullRequests PullRequests { get; set; }
        public Commits Commits { get; set; }
        public Items Items { get; set; }
        public Pushes Pushes { get; set; }
    }
}
