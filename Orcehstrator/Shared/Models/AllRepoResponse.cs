 namespace DevOps.TaskMaster.Orchestrator.Shared.Models
 {
    public class Repository
    {
        public string Name { get; set; } 
        public string Id { get; set; } 
        public string Url { get; set; } 
        public object Project { get; set; } 
        public Error Error { get; set; } 
    }

    public class Template    
    {
        public string BuildAgentName { get; set; } 
        public string TemplateBuildName { get; set; }
        public string TemplateReleaseName { get; set; }
        public string Name { get; set; } 
        public Error Error { get; set; } 
    }

    public class RepoResponse  
    {
        public Repository Repo { get; set; } 
        public string ProjectName { get; set; } 
        public string Status { get; set; } 
        public Template Template { get; set; } 
        public Error Error { get; set; } 
        public object Integration { get; set; } 
    }

    public class Error
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }
}