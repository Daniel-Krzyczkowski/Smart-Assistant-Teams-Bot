using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using TMF.SmartAssistant.FuncApps.Core.DependencyInjection;

[assembly: FunctionsStartup(typeof(TMF.SmartAssistant.FuncApps.Startup))]
namespace TMF.SmartAssistant.FuncApps
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddAppConfiguration();
            builder.Services.AddMsGraphSdkClientServices();
            builder.Services.AddDatabaseServices();
            builder.Services.AddBotAuthenticationServices();

        }
    }
}
