
namespace DevOps.TaskMaster.Orchestrator.Shared.Models
{
    public class CreateReleaseRequestDto
    {
        public string releaseProject { get; set; }
        public string buildName { get; set; }
        public string repoName { get; set; }
        public string templateName { get; set; }
    }
}