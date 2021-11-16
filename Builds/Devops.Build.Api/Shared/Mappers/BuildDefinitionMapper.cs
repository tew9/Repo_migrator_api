using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DevOps.Build.Contracts;
using DevOps.Build.Api.Shared.Models;

namespace DevOps.Build.Api.Shared.Mappers
{
    public class BuildDefinitionMapper : IMapper<BuildDefinition, BuildDefinitionDto>
    {
        public Task<BuildDefinition> Map(BuildDefinitionDto from)
        {
            throw new NotImplementedException();
        }

        public async Task<BuildDefinitionDto> Map(BuildDefinition from)
        {
            return new BuildDefinitionDto()
            {
                Id = from.Id,
                Revision = from.Revision,
                Name = from.Name,
               Path=from.Path   
            };
        }
    }
}