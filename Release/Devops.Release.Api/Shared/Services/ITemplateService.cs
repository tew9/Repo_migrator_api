using System.Threading.Tasks;
using DevOps.Release.Contracts;

namespace DevOps.Release.Api.Shared.Services
{
  public interface ITemplateService
  {
    Task<ApplicationTemplateDto> GetTemplate(string templateName);
  }
}