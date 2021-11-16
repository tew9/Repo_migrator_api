using System.Threading.Tasks;
using DevOps.Repo.Api.Shared.Models.TableEntities;
using System.Collections.Generic;

namespace DevOps.Repo.Api.Shared.Services
{
  public interface ITemplateService
  {
    Task<ApplicationTemplate> GetTemplate(string templateName);
    Task<List<ApplicationTemplate>> GetAllTemplates();
  }
}