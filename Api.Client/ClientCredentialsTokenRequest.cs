namespace Api.Client
{
    internal class ClientCredentialsTokenRequest
    {
        public object Address { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
    }
}