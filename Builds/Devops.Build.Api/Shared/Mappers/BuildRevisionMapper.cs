using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DevOps.Build.Contracts;
using DevOps.Build.Api.Shared.Models;

namespace DevOps.Build.Api.Shared.Mappers
{
    public class BuildRevisionMapper : IMapper<BuildRevision, BuildRevisionDto>
    {
        public Task<BuildRevision> Map(BuildRevisionDto from)
        {
            throw new NotImplementedException();
        }

        public async Task<BuildRevisionDto> Map(BuildRevision from)
        {
            return new BuildRevisionDto()
            {
                Id = from.Revision,
                Name = from.Name,
                ChangedDate = from.ChangedDate
            };
        }
    }
}