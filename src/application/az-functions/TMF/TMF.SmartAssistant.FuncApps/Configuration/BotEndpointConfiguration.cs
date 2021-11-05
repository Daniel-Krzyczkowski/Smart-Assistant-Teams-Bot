namespace TMF.SmartAssistant.FuncApps.Configuration
{
    internal interface IBotEndpointConfiguration
    {
        string NotificationEndpointUrl { get; set; }
    }

    internal class BotEndpointConfiguration : IBotEndpointConfiguration
    {
        public string NotificationEndpointUrl { get; set; }
    }
}
