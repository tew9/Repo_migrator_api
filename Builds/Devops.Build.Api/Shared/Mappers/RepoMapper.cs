using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DevOps.Build.Contracts;
using DevOps.Build.Api.Shared.Models;

namespace DevOps.Build.Api.Shared.Mappers
{
    public class RepoMapper : IMapper<Repo, RepoDto>
    {
        public Task<Repo> Map(RepoDto from)
        {
            throw new NotImplementedException();
        }

        public async Task<RepoDto> Map(Repo from)
        {
            return new RepoDto()
            {
                Id = from.Id,
                Name = from.Name,
                ProjectId = from.Project.Id
            };
        }
    }
}