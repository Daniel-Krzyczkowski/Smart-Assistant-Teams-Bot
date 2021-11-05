namespace TMF.SmartAssistant.FuncApps.Configuration
{
    internal interface IMsGraphServiceConfiguration
    {
        string TenantId { get; set; }
        string AppId { get; set; }
        string AppSecret { get; set; }
    }

    internal class MsGraphServiceConfiguration : IMsGraphServiceConfiguration
    {
        public string TenantId { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }
}
