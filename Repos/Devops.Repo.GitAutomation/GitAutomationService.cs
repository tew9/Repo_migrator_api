using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace DevOps.Repo.GitAutomation
{
  public class GitAutomationService: IGitAutomationService
  {
    #region Instance Variables & Constants 
    private string _urlTfvc;
    private string _urlTemplate; 
    private string _userName; 
    private string _password;  
    private string[] _extensions;
    private string _signatureEmail;
    private string _Workingdir;
    private FormatDirectoryService _dirService;
    #endregion

    #region Constructor
    public GitAutomationService(string urlTfvc, string urlTemplate, 
                                string localWorkdir, string userName, string password,
                                string[] extensions, string signatureEmail)
    {
      _userName = userName;
      _signatureEmail = signatureEmail;
      _password = password;
      _urlTfvc = urlTfvc;
      _urlTemplate = urlTemplate;
      _Workingdir = localWorkdir;
      _extensions = extensions;
      _dirService = new FormatDirectoryService();
    }
    #endregion

    #region Unique Directories

    public string getNextDirName(string path, string name)
    {
      string AliasName = string.Empty;
      string fullPath = Path.Combine(path, name);

            // If the Folder doesn't exist then create and save the file
      string[] directories = Directory.GetDirectories(path);

      if (directories.Contains(fullPath))
      {
        string sn = null;
        for (int i = 1; i < directories.Count() + 1; i++)
        {
            sn = fullPath + "(" + i + ")";
            if (directories.Contains(sn))
            {
              continue;
            }
            else
            {
              return sn;
            }
        }
        return sn;
      }
      else
      {
        return fullPath;
      }
    }
    #endregion

    #region Clone
    public void CloneRepo(string remoteUrl, string workDir, Credentials cred)
    {
      var cloneOptions = new CloneOptions();
      cloneOptions.CredentialsProvider = (_url, _user, _cred) => cred;
      try
      {
        Repository.Clone(remoteUrl, workDir, cloneOptions);
      }
      catch (Exception e)
      {
        throw e;
      }
    }
    #endregion
  
    #region fetch
    public void FetchRemoteRepo(Repository repo, string remoteName, CredentialsHandler ch)
    {
      var fetchOption = new FetchOptions { CredentialsProvider = ch };
      try
      {
        var remote = repo.Network.Remotes[remoteName];
        var refSpec = remote.FetchRefSpecs.Select(x => x.Specification);
        repo.Network.Fetch(remoteName, refSpec, fetchOption);
      }
      catch (Exception e)
      {
        throw e;
      }
    }
    #endregion
    
    #region UpdateTrackingBranch
    public  void UpdateTrackingBranch(Branch newBranchRef, Repository repo)
    {
      try
      {
        if(newBranchRef.IsRemote)
        {
          repo.Branches.Update(repo.Head, b => b.Remote = newBranchRef.RemoteName,
                                                    b => b.UpstreamBranch = newBranchRef.FriendlyName,
                                                    b => b.TrackedBranch = newBranchRef.CanonicalName); 
        }
      }
      catch(Exception e)
      {
        throw e;
      }
      
    }
    #endregion

    #region Pull
    public void Pull(Repository repo, CredentialsHandler ch, Signature signature)
    {
        try
        {
          PullOptions options = new PullOptions();
          var fetchOption = new FetchOptions { CredentialsProvider = ch };
          options.FetchOptions = fetchOption;
          options.MergeOptions = new MergeOptions()
          {
            FastForwardStrategy = FastForwardStrategy.Default
          };
                        
          var result = Commands.Pull(repo, signature, options);
        }
        catch (Exception e)
        {
          throw e;
        }
    }
    #endregion

    #region Commit
    public bool Commit(Repository repo, Signature signature, string commitMessage)
    {
      try
      {
        Commands.Stage(repo, "*");
        var committer = signature;
        repo.Commit(commitMessage, signature, committer);
        return true;
      }
      catch(Exception e)
      {
        if(e.Message == "No changes; nothing to commit.")
        {
         return true;
        }
        else
        {
          return false;       
        }
      }
    }
    #endregion
    
    #region Merge
    public void Merge(Repository repo, Signature signature, Branch trackingBranch)
    {
      try
      {
        var head = repo.Head;
        MergeOptions opts = new MergeOptions() { FileConflictStrategy = CheckoutFileConflictStrategy.Merge };
        repo.Merge(trackingBranch, signature, opts);
      }
      catch(Exception e)
      {
        throw e;
      }
     
    }
    #endregion
  
    #region Push
    public void Push(Repository repo, CredentialsHandler ch)
    {
      try
      {
        var pushOptions = new PushOptions();
        pushOptions.CredentialsProvider = ch;
        repo.Network.Push(repo.Head, pushOptions);
      }
      catch(Exception e)
      {
        throw e;
      }
    }
    #endregion

    #region Point the head back to origin
    public void PointBackToOrigin(Repository repo, Dictionary<string, string> Message)
    {
      try
      {
        //update the head from pointing to template branch to pointing tfvc branch origin/master
        //in case the tfvc repo is an empty repo in which we just putting the template into, add new remote
        if (repo.Branches["refs/remotes/origin/master"] == null)
        {
          dynamic remote2 = null;
          if(repo.Branches["refs/heads/master"].RemoteName == "origin")
          {
            remote2 = repo.Network.Remotes["origin"];
          }
          else
          {
            remote2 = repo.Network.Remotes.Add("origin", _urlTfvc);
          }
          repo.Branches.Update(repo.Head, b => b.Remote = remote2.Name,
                                              b => b.UpstreamBranch = remote2.Name,
                                                b => b.TrackedBranch = "refs/remotes/origin/master");
          Message.Add("7. update2", "Changed Head to origin from remote ref of origin Successfuly");
        }
        else
        {
          var originalRemoteBranch = repo.Branches["refs/remotes/origin/master"];
          UpdateTrackingBranch(originalRemoteBranch, repo);
          Message.Add("7. update2", "Head is changed to point to tfvc origin/master Succesfuly");
        }
      }
      catch(Exception e)
      {
        throw e;
      }
    }
        
    #endregion
  
    #region IntegrateTemplate
    public Dictionary<string, string> IntegrateTemplate(string commitMessage, bool cleanUpDir, bool integrateTemplate)
    {
      var localWorkdir =  getNextDirName(_Workingdir, "AutomationClonnedRepo");
      DirectoryInfo dir = new DirectoryInfo(localWorkdir);
      Dictionary<string, string> Message = new Dictionary<string, string>();
      var creds = new UsernamePasswordCredentials { Username = _userName, Password = _password };
      CredentialsHandler ch = (_url, _user, _cred) => creds;

      var signature = new Signature( new Identity(_signatureEmail.Split("@")[0], _signatureEmail), DateTimeOffset.Now);

      try
      {
        if(!Directory.Exists(localWorkdir))
        {
          CloneRepo(_urlTfvc, localWorkdir, creds);
          Message.Add("1. clone", "Successfully Cloned!!!");
        }

        if (dir.GetDirectories().Length <= 0 || dir.GetFiles().Length <= 0)
        {
          Message.Add("Warning", $"{localWorkdir} exist but has no files in it, it's preventing the template integration to be done. Please delete the directory and perform the cleaning again.");
          return Message;
        }

        else
        {
          using (var repo = new Repository(localWorkdir))
          {
            string templateOrigin = "origin2";
            //Create second remote for templateRepo
            if(repo.Network.Remotes[templateOrigin] == null) 
              repo.Network.Remotes.Add(templateOrigin, _urlTemplate);
          
            if(integrateTemplate)
            {
              Branch trackingBranch;
              if (repo.Branches[$"refs/remotes/{templateOrigin}/master"] == null)
              {
                //fetch the remote template Repo head, to create new branch
                FetchRemoteRepo(repo, templateOrigin, ch);  
                trackingBranch = repo.Branches[$"refs/remotes/{templateOrigin}/master"];
                Message.Add("2. fetch", "Fetch Template Repo master branch or origin2/master");   
              }
              else
              {
                //it's already a branch or created
                trackingBranch = repo.Branches[$"refs/remotes/{templateOrigin}/master"];
                Message.Add("3. trackingBranch", "create temporary branch that points to template origin2/master"); 
              }

               //update the upstream of the repo Head to point to the template repo remote
              if (trackingBranch.IsRemote)
              {
                UpdateTrackingBranch(trackingBranch, repo);
                Message.Add("4. update1", "change Head origin/master to point to template origin2/master.");
              }

               //If template files have never been integrated into the repo, do the merge from the configured upstream
              // with the current master
              if (!File.Exists(localWorkdir + @"\deploy\azure-pipelines.yml"))
              {
                Pull(repo, ch, signature);
                Message.Add("5. pull", "Pulled template files and merge them with local head Successfully!!!");
              }
            
              //If there's a merge conflit happens resolve the merge it and commit it.
              // merge it
              if (!repo.Index.IsFullyMerged)
              {

                if(Commit(repo, signature, commitMessage))
                {
                  Message.Add("6. commit", "Commited template changes in the current temporary local head successfuly");
                
                  //merge the changes from trackingbranch into repo.head pointing to origin master
                  Merge(repo, signature, trackingBranch);
                  Message.Add("6. merge", "Resolve Merged Conflict Succesfuly!!!");
                }
                else
                {
                  Message.Add("6. commit", "No Changes, There's nothing to commit");
                }
              }
            }

            //point origin back to origin/master
            PointBackToOrigin(repo, Message);

            //Cleanup TFVCs
            if(cleanUpDir && dir.Exists)
            {
              foreach(var ext in _extensions)
              {
                _dirService.FormatDirectory(dir, ext);
              }
              Message.Add("8. format", $"Formatted and Removed the unwanted files in local directory Succesfuly");
            }

            //Check if the directory is formated and if any other change is made, if so, stage it commit it and push again
            foreach(var status in repo.RetrieveStatus())
            {
              if(!string.IsNullOrEmpty(status.FilePath))
              {
                if(Commit(repo, signature, commitMessage))
                {
                  Message.Add("6. commit", "Commited template changes in the current temporary local head successfuly");
                  break;
                }
                else
                {
                  Message.Add("6. commit", "No Changes, There's nothing to commit"); 
                }
              }
            }

            //push everything back
            Push(repo, ch);
            Message.Add("9. Push", "Push all changes to tfvc git repo origin/master");

            repo.Network.Remotes.Remove("origin");

            if(dir.Exists)
            {
              _dirService.DeleteTempDirectory(dir);
              dir.Delete(true);
            }
              
            return Message;
          }

        }
      }
      catch (Exception ex)
      {
        Message.Add("11. error", ex.Message);
        return Message;
      }
    }
  }
  #endregion
}
