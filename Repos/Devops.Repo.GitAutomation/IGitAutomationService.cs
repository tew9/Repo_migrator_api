using System.Collections.Generic;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace DevOps.Repo.GitAutomation
{
  public interface IGitAutomationService
  {
    public void CloneRepo(string remoteUrl, string workDir, Credentials cred);
    public void FetchRemoteRepo(Repository repo, string remoteName, CredentialsHandler credential);
    public void UpdateTrackingBranch(Branch trackingBranch, Repository repo);
    public void Pull(Repository repo, CredentialsHandler ch,  Signature signature);
    public bool Commit(Repository repo, Signature signature, string commitMessage);
    public void Push(Repository repo, CredentialsHandler ch);
    public Dictionary<string, string> IntegrateTemplate(string commitMessage, bool isCleaningUpAllowed, bool isJustIntegration);
  }
}