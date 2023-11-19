namespace WhoIsLive.Lib.Events.Authentication
{
    public class OAuth2AuthenticationEventArgs : EventArgs
    {
        public OAuth2AuthenticationEventArgs(string clientId, string accessToken, string type)
        {
            ClientId = clientId;
            AccessToken = accessToken;
            Type = type;
        }

        public string ClientId { get; }
        public string AccessToken { get; }
        public string Type { get; }
    }
}