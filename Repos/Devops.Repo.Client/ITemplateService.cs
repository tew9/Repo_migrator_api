using System.Net.Http;
using System.Threading.Tasks;

namespace DevOps.Repo.Client
{
  public interface ITemplateService
  {
    Task<HttpResponseMessage> GetTemplate(string templateName);
    Task<HttpResponseMessage> GetAllTemplates();
  }
}
