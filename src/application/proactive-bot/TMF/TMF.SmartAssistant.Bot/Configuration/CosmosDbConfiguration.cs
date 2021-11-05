namespace TMF.SmartAssistant.Bot.Configuration
{
    public class CosmosDbConfiguration
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string BotConversationReferenceContainerName { get; set; }
        public string BotConversationReferenceContainerPartitionKeyPath { get; set; }
    }
}
