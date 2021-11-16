namespace TaskMaster.Common.Client
{
    public class ClientConfig : IClientConfig
    {
        public string ApplicationName { get; set; }
        public string OktaClientId { get; set; }
        public string OktaClientSecret { get; set; }
        public string OktaTokenUrl { get; set; }
        public string DevOpsApiBaseUrl { get; set; }
    }
}