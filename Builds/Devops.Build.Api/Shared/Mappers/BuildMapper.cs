using DevOps.Build.Contracts;
using DevOps.Build.Api.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevOps.Build.Contracts;

namespace DevOps.Build.Api.Shared.Mappers
{
    public class BuildMapper : IMapper<Builds, BuildDto>
    {
        public Task<Builds> Map(BuildDto from)
        {
            throw new NotImplementedException();
        }

        public async Task<BuildDto> Map(Builds from)
        {
            return new BuildDto()
            {
                Id = from.Id,
                Status = from.Status,
                Result = from.Result
            };
        }
    }
}