using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web;
using DevOps.Build.Api.Shared.TableEntities;
using DevOps.Build.Contracts;


namespace DevOps.Build.Api.Shared.Mappers
{
    public class ApplicationTemplateMapper : IMapper<ApplicationTemplate, ApplicationTemplateDto>
    {
        public Task<ApplicationTemplate> Map(ApplicationTemplateDto from)
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationTemplateDto> Map(ApplicationTemplate from)
        {
            return new ApplicationTemplateDto()
            {
                Name = from.RowKey,
                BuildAgentName = from.BuildAgentName,
                GitUrl = from.GitUrl,
                Platform = from.Platform,
                ReleaseDefinitionId = from.ReleaseDefinitionId,
                BuildName = from.BuildName,
                ReleaseName = from.ReleaseName,
                RepoName = from.RepoName,
                RepoId = from.RepoId
            };
        }
    }
}