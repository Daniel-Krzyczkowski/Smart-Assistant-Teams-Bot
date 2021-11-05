using Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using TMF.SmartAssistant.FuncApps.Configuration;
using TMF.SmartAssistant.FuncApps.Infrastructure.Data;

namespace TMF.SmartAssistant.FuncApps.Core.DependencyInjection
{
    internal static class DataServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
        {
            services.AddSingleton(implementationFactory =>
            {
                var cosmoDbConfiguration = implementationFactory.GetRequiredService<ICosmosDbConfiguration>();
                CosmosClient cosmosClient = new CosmosClient(cosmoDbConfiguration.ConnectionString);
                CosmosDatabase database = cosmosClient.CreateDatabaseIfNotExistsAsync(cosmoDbConfiguration.DatabaseName)
                                                       .GetAwaiter()
                                                       .GetResult();

                database.CreateContainerIfNotExistsAsync(
                    cosmoDbConfiguration.EmployeeOverTimeRecordContainerName,
                    cosmoDbConfiguration.EmployeeOverTimeRecordContainerPartitionKeyPath,
                    400)
                    .GetAwaiter()
                    .GetResult();

                return cosmosClient;
            });

            services.AddSingleton<IOrganizationMemberOverTimeRecordRepository, OrganizationMemberOverTimeRecordRepository>();

            return services;
        }
    }
}
