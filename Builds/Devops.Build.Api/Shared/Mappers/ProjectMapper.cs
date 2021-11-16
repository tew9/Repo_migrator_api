using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DevOps.Build.Contracts;
using DevOps.Build.Api.Shared.Models;

namespace DevOps.Build.Api.Shared.Mappers
{
    public class ProjectMapper : IMapper<Project, ProjectDto>
    {
        public Task<Project> Map(ProjectDto from)
        {
            throw new NotImplementedException();
        }

        public async Task<ProjectDto> Map(Project from)
        {
            return new ProjectDto()
            {
                Id = from.Id,
                Name = from.Name
            };
        }
    }
}