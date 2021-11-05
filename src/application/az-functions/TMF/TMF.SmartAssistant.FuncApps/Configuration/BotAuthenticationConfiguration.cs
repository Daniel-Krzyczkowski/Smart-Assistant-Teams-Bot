namespace TMF.SmartAssistant.FuncApps.Configuration
{
    internal interface IBotAuthenticationConfiguration
    {
        string Authority { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string RedirectUri { get; set; }
        string Scope { get; set; }
    }

    internal class BotAuthenticationConfiguration : IBotAuthenticationConfiguration
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string Scope { get; set; }
    }
}
