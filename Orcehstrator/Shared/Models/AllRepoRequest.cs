
namespace DevOps.TaskMaster.Orchestrator.Shared.Models
{
    public class RepoCreateTemplateRequest
    {
        public string destinationRepoName { get; set; }
        public string templateName { get; set; }
        public string destinationProjectName { get; set; }
        public string? comitterEmail { get; set; }
        public bool cleanUp { get; set; }
        public bool integration { get; set; }
        public bool build { get; set; }
        public bool release { get; set; }
    }

    public class RepoCreateEmptyRequest
    {
        public string destinationRepoName { get; set; }
        public string destinationProjectName { get; set; }
    }

    public class TfvcSource 
   {
        public string path { get; set; } 
        public bool importHistory { get; set; } 
        public int importHistoryDurationInDays { get; set; } 
    }

    public class Tfvc2GitRequest
    {
        public string destinationRepoName { get; set; }
        public string repoType { get; set; } 
        public TfvcSource tfvcSource { get; set; } 
        public string sourceProjectName { get; set; }
        public string gitSourceRepoName { get; set; }
        public string destinationProjectName { get; set; }
    }

    public class Git2GitRequest
    {
        public string sourceProjectName { get; set; }
        public string gitSourceRepoName { get; set; }
        public string destinationProjectName { get; set; }
        public string destinationRepoName { get; set; }
    }
}
