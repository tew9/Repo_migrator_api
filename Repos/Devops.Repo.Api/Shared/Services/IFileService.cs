using System.Threading.Tasks;
using DevOps.Repo.Api.Shared.Models.Git;

namespace DevOps.Repo.Api.Shared.Services
{
  public interface IFileService
  {
    public Change RenameItem(string oldName, string newName);
    public Change FindReplaceContent(string itemPath, string oldValue, string newValue);
  }
}