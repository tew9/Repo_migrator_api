using DevOps.Release.Contracts;

namespace DevOps.Release.Api.Shared.Models
{
    public class ReleaseDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ErrorDto Error { get; set; }
    }
}