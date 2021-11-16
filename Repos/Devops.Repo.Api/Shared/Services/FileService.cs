using DevOps.Repo.Api.Shared.Models.Git;

namespace DevOps.Repo.Api.Shared.Services
{
  public class FileService : IFileService
  {
    public Change RenameItem(string oldName, string newName)
    {
      string newPath = oldName.Replace("UniqueNameGoesHere", newName);
      newPath = newPath.Replace("uniquenamegoeshere", newName.ToLower());

      var change = new Change()
      {
        ChangeType =  "rename",
        SourceServerItem = oldName,
        Item = new Item() { Path = newPath }
      };

      return change;
    }

    public Change FindReplaceContent(string itemPath, string oldValue, string newValue)
    {
      string newContent = oldValue.Replace("UniqueNameGoesHere", newValue);
      newContent = newContent.Replace("uniquenamegoeshere", newValue.ToLower());

      var change = new Change()
      {
        ChangeType = "edit",
        Item = new Item() { Path = itemPath },
        NewContent = new ItemContent() { Content = newContent, ContentType = "rawtext" }
      };

      return change;
    }
  }
}