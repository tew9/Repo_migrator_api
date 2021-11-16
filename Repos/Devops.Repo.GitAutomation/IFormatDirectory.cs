
using System.IO;

namespace DevOps.Repo.GitAutomation
{
  public interface IFormatDirectoryService
  {
    bool FormatDirectory(DirectoryInfo dir, string fileExt);
    void DeleteTempDirectory(DirectoryInfo directory);
  }
}