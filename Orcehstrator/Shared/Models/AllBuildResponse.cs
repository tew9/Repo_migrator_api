
using System.Collections.Generic;

namespace DevOps.TaskMaster.Orchestrator.Shared.Models
{
    public class BuildDefinitionList
    {
        public string Count { get; set; }
        public List<BuildDefinition> Value { get; set; }
       public Error Error { get; set; }
    }

    public class BuildDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Error Error { get; set; }
        public int Revision { get; set; }
        public string Path { get; set; }
    }
}