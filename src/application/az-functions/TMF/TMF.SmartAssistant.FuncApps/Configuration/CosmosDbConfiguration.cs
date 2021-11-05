namespace TMF.SmartAssistant.FuncApps.Configuration
{
    internal interface ICosmosDbConfiguration
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string EmployeeOverTimeRecordContainerName { get; set; }
        string EmployeeOverTimeRecordContainerPartitionKeyPath { get; set; }
    }

    internal class CosmosDbConfiguration : ICosmosDbConfiguration
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string EmployeeOverTimeRecordContainerName { get; set; }
        public string EmployeeOverTimeRecordContainerPartitionKeyPath { get; set; }
    }
}
