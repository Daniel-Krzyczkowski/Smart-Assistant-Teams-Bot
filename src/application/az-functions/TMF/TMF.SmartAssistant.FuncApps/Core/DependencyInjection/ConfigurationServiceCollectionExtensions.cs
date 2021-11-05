using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using TMF.SmartAssistant.FuncApps.Configuration;

namespace TMF.SmartAssistant.FuncApps.Core.DependencyInjection
{
    internal static class ConfigurationServiceCollectionExtensions
    {
        public static IServiceCollection AddAppConfiguration(this IServiceCollection services)
        {
            IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            services.Configure<MsGraphServiceConfiguration>(configuration.GetSection("MsGraphServiceConfiguration"));
            var msGraphServiceConfiguration = services.BuildServiceProvider()
                                                      .GetRequiredService<IOptions<MsGraphServiceConfiguration>>().Value;

            if (string.IsNullOrEmpty(msGraphServiceConfiguration.AppId))
            {
                throw new ArgumentNullException($"Missing configuration parameter for MS Graph Service: {nameof(msGraphServiceConfiguration.AppId)}");
            }

            if (string.IsNullOrEmpty(msGraphServiceConfiguration.AppSecret))
            {
                throw new ArgumentNullException($"Missing configuration parameter for MS Graph Service: {msGraphServiceConfiguration.AppSecret}");
            }

            if (string.IsNullOrEmpty(msGraphServiceConfiguration.TenantId))
            {
                throw new ArgumentNullException($"Missing configuration parameter for MS Graph Service: {nameof(msGraphServiceConfiguration.TenantId)}");
            }

            services.AddSingleton<IMsGraphServiceConfiguration>(msGraphServiceConfiguration);


            services.Configure<MsTeamsAppConfiguration>(configuration.GetSection("MsTeamsAppConfiguration"));
            var msTeamsAppConfiguration = services.BuildServiceProvider()
                                                      .GetRequiredService<IOptions<MsTeamsAppConfiguration>>().Value;

            if (string.IsNullOrEmpty(msTeamsAppConfiguration.AppId))
            {
                throw new ArgumentNullException($"Missing configuration parameter for MS Teams App: {msTeamsAppConfiguration.AppId}");
            }

            services.AddSingleton<IMsTeamsAppConfiguration>(msTeamsAppConfiguration);


            services.Configure<BotAuthenticationConfiguration>(configuration.GetSection("BotAuthenticationConfiguration"));
            var botAuthenticationConfiguration = services.BuildServiceProvider()
                                                      .GetRequiredService<IOptions<BotAuthenticationConfiguration>>().Value;

            if (string.IsNullOrEmpty(botAuthenticationConfiguration.Authority))
            {
                throw new ArgumentNullException($"Missing configuration parameter for Bot Authentication configuration: {nameof(botAuthenticationConfiguration.Authority)}");
            }

            if (string.IsNullOrEmpty(botAuthenticationConfiguration.ClientId))
            {
                throw new ArgumentNullException($"Missing configuration parameter for Bot Authentication configuration: {nameof(botAuthenticationConfiguration.ClientId)}");
            }

            if (string.IsNullOrEmpty(botAuthenticationConfiguration.ClientId))
            {
                throw new ArgumentNullException($"Missing configuration parameter for Bot Authentication configuration: {nameof(botAuthenticationConfiguration.ClientId)}");
            }

            if (string.IsNullOrEmpty(botAuthenticationConfiguration.RedirectUri))
            {
                throw new ArgumentNullException($"Missing configuration parameter for Bot Authentication configuration: {nameof(botAuthenticationConfiguration.RedirectUri)}");
            }

            if (string.IsNullOrEmpty(botAuthenticationConfiguration.Scope))
            {
                throw new ArgumentNullException($"Missing configuration parameter for Bot Authentication configuration: {nameof(botAuthenticationConfiguration.Scope)}");
            }

            services.AddSingleton<IBotAuthenticationConfiguration>(botAuthenticationConfiguration);


            services.Configure<CosmosDbConfiguration>(configuration.GetSection("CosmosDbConfiguration"));
            var cosmosDbConfiguration = services.BuildServiceProvider()
                                                      .GetRequiredService<IOptions<CosmosDbConfiguration>>().Value;

            if (string.IsNullOrEmpty(cosmosDbConfiguration.ConnectionString))
            {
                throw new ArgumentNullException($"Missing configuration parameter for Cosmos DB configuration: {nameof(cosmosDbConfiguration.ConnectionString)}");
            }

            if (string.IsNullOrEmpty(cosmosDbConfiguration.DatabaseName))
            {
                throw new ArgumentNullException($"Missing configuration parameter for Cosmos DB configuration: {nameof(cosmosDbConfiguration.DatabaseName)}");
            }

            if (string.IsNullOrEmpty(cosmosDbConfiguration.EmployeeOverTimeRecordContainerName))
            {
                throw new ArgumentNullException($"Missing configuration parameter for Cosmos DB configuration: {nameof(cosmosDbConfiguration.EmployeeOverTimeRecordContainerName)}");
            }

            if (string.IsNullOrEmpty(cosmosDbConfiguration.EmployeeOverTimeRecordContainerPartitionKeyPath))
            {
                throw new ArgumentNullException($"Missing configuration parameter for Cosmos DB configuration: {nameof(cosmosDbConfiguration.EmployeeOverTimeRecordContainerPartitionKeyPath)}");
            }

            services.AddSingleton<ICosmosDbConfiguration>(cosmosDbConfiguration);


            services.Configure<BotEndpointConfiguration>(configuration.GetSection("BotEndpointConfiguration"));
            var botEndpointConfiguration = services.BuildServiceProvider()
                                                      .GetRequiredService<IOptions<BotEndpointConfiguration>>().Value;

            if (string.IsNullOrEmpty(botEndpointConfiguration.NotificationEndpointUrl))
            {
                throw new ArgumentNullException($"Missing configuration parameter for Bot notification endpoint: {botEndpointConfiguration.NotificationEndpointUrl}");
            }

            services.AddSingleton<IBotEndpointConfiguration>(botEndpointConfiguration);


            return services;
        }
    }
}
