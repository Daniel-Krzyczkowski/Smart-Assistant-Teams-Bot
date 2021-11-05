using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using TMF.SmartAssistant.FuncApps.Configuration;
using TMF.SmartAssistant.FuncApps.Infrastructure.Services.MsGraph;

namespace TMF.SmartAssistant.FuncApps.Core.DependencyInjection
{
    internal static class MsGraphServiceCollectionExtensions
    {
        public static IServiceCollection AddMsGraphSdkClientServices(this IServiceCollection services)
        {
            services.AddSingleton(implementationFactory =>
            {
                var msGraphServiceConfiguration = implementationFactory.GetRequiredService<IMsGraphServiceConfiguration>();
                IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(msGraphServiceConfiguration.AppId)
                    .WithTenantId(msGraphServiceConfiguration.TenantId)
                    .WithClientSecret(msGraphServiceConfiguration.AppSecret)
                    .Build();

                ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
                return new GraphServiceClient(authProvider);
            });

            services.AddSingleton<IMsGraphSdkClientService, MsGraphSdkClientService>();

            return services;
        }
    }
}
