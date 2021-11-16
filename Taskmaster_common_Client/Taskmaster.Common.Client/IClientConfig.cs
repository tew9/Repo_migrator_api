namespace TaskMaster.Common.Client
{
    public interface IClientConfig
    {
        string ApplicationName { get; }
        string OktaClientId { get; }
        string OktaClientSecret { get; }
        string OktaTokenUrl { get; }
        string DevOpsApiBaseUrl { get; }
    }
}