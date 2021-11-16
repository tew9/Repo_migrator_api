using DevOps.Release.Contracts;

namespace DevOps.Release.Api.Shared.Models
{
    public class BuildDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Revision { get; set; }
        public string Url { get; set; }
        public ProjectDto Project { get; set; }
        public BuildQueueDto Queue { get; set; }
    }
}