using System;
using System.IO;

namespace DevOps.Repo.GitAutomation
{
  public class FormatDirectoryService
  {
    public void FormatDirectory(DirectoryInfo dir, string fileExt)
    {  
      try
      {
        foreach (var subDir in dir.GetDirectories())
        {
          if(subDir.Name.Equals(".git"))
          {
            continue;
          }
          FormatDirectory(subDir, fileExt);
        }
        FileInfo[] files = dir.GetFiles("*."+fileExt);
        if(files.Length > 0)
        {
          foreach (var file in files)
          {
            file.Attributes = FileAttributes.Normal;
            File.Delete(file.FullName);
          }
        }
      } 
      catch(Exception e)
      {
        throw e;
      }
    }

    public void DeleteTempDirectory(DirectoryInfo dir)
    {
      foreach (var subDir in dir.GetDirectories())
        DeleteTempDirectory(subDir);

      try
      {
        foreach (var file in dir.GetFiles())
        {
          file.Attributes = FileAttributes.Normal;
        }
      }
      catch(Exception e)
      {
        throw e;
      }
      
    }
  }
}