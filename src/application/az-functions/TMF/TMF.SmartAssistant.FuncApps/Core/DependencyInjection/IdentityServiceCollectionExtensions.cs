using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using TMF.SmartAssistant.FuncApps.Configuration;

namespace TMF.SmartAssistant.FuncApps.Core.DependencyInjection
{
    internal static class IdentityServiceCollectionExtensions
    {
        public static IServiceCollection AddBotAuthenticationServices(this IServiceCollection services)
        {
            services.AddScoped(implementationFactory =>
            {
                var botAuthenticationConfiguration = implementationFactory.GetRequiredService<IBotAuthenticationConfiguration>();
                var confidentialClient = ConfidentialClientApplicationBuilder.Create(botAuthenticationConfiguration.ClientId)
                    .WithClientSecret(botAuthenticationConfiguration.ClientSecret)
                    .WithAuthority(botAuthenticationConfiguration.Authority)
                    .WithRedirectUri(botAuthenticationConfiguration.RedirectUri)
                    .Build();

                return confidentialClient;
            });

            return services;
        }
    }
}
