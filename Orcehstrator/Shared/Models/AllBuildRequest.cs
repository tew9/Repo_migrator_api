namespace DevOps.TaskMaster.Orchestrator.Shared.Models
{
    public class CreateBuildRequest
    { 
        public string buildName { get; set; }
        public string projectName { get; set; }
        public string repoId { get; set; }
        public string buildAgentName { get; set; }
        public string templateBuildName { get; set; }
    }
}
