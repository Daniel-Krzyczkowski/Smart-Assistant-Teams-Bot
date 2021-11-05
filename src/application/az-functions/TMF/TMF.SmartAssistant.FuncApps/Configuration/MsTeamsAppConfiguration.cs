namespace TMF.SmartAssistant.FuncApps.Configuration
{
    internal interface IMsTeamsAppConfiguration
    {
        string AppId { get; set; }
    }

    internal class MsTeamsAppConfiguration : IMsTeamsAppConfiguration
    {
        public string AppId { get; set; }
    }
}
