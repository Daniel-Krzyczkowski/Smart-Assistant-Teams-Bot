using Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TMF.SmartAssistant.Bot.Configuration;
using TMF.SmartAssistant.Bot.Repositories;

namespace TMF.SmartAssistant.Bot.Core.DependencyInjection
{
    internal static class DataServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
        {
            services.AddSingleton(implementationFactory =>
            {
                var cosmoDbConfiguration = implementationFactory.GetRequiredService<IOptions<CosmosDbConfiguration>>().Value;
                CosmosClient cosmosClient = new CosmosClient(cosmoDbConfiguration.ConnectionString);
                CosmosDatabase database = cosmosClient.CreateDatabaseIfNotExistsAsync(cosmoDbConfiguration.DatabaseName)
                                                       .GetAwaiter()
                                                       .GetResult();

                database.CreateContainerIfNotExistsAsync(
                    cosmoDbConfiguration.BotConversationReferenceContainerName,
                    cosmoDbConfiguration.BotConversationReferenceContainerPartitionKeyPath,
                    400)
                    .GetAwaiter()
                    .GetResult();

                return cosmosClient;
            });

            services.AddSingleton<IConversationReferenceRepository, ConversationReferenceRepository>();

            return services;
        }
    }
}
