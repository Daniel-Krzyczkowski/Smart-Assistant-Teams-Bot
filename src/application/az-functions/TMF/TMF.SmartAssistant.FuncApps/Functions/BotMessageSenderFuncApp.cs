using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using TMF.SmartAssistant.FuncApps.Application.Model;
using TMF.SmartAssistant.FuncApps.Configuration;
using TMF.SmartAssistant.FuncApps.Infrastructure.Services.MsGraph;
using TMF.SmartAssistant.FuncApps.Utils;

namespace TMF.SmartAssistant.FuncApps.Functions
{
    internal class BotMessageSenderFuncApp
    {
        private readonly IConfidentialClientApplication _confidentialClient;
        private readonly IBotAuthenticationConfiguration _botAuthenticationConfiguration;
        private readonly IBotEndpointConfiguration _botEndpointConfiguration;
        private readonly HttpClient _httpClient;
        private readonly IMsGraphSdkClientService _msGraphSdkClientService;

        public BotMessageSenderFuncApp(IConfidentialClientApplication confidentialClient,
                                       IBotAuthenticationConfiguration botAuthenticationConfiguration,
                                       IBotEndpointConfiguration botEndpointConfiguration,
                                       HttpClient httpClient,
                                       IMsGraphSdkClientService msGraphSdkClientService)
        {
            _confidentialClient = confidentialClient
                                        ?? throw new ArgumentNullException(nameof(confidentialClient));
            _botAuthenticationConfiguration = botAuthenticationConfiguration
                                        ?? throw new ArgumentNullException(nameof(botAuthenticationConfiguration));
            _botEndpointConfiguration = botEndpointConfiguration
                                        ?? throw new ArgumentNullException(nameof(botEndpointConfiguration));
            _httpClient = httpClient
                                        ?? throw new ArgumentNullException(nameof(httpClient));
            _msGraphSdkClientService = msGraphSdkClientService
                                        ?? throw new ArgumentNullException(nameof(msGraphSdkClientService));
        }

        [FunctionName(FunctionNamesRepository.BotMessageSenderFuncAppName)]
        public async Task SendNotificationToOrganizationMembersOnTeamsAsync([ActivityTrigger] List<EmployeeOverTimeRecord> organizationMembersWithExtendedWorkingTime)
        {
            AuthenticationResult result = await _confidentialClient
                                   .AcquireTokenForClient(new string[] { _botAuthenticationConfiguration.Scope })
                                   .ExecuteAsync();
            var accessToken = result.AccessToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            foreach (var employeeOverTimeRecord in organizationMembersWithExtendedWorkingTime)
            {
                var userMsTeamsBotAppInstallationId = await _msGraphSdkClientService
                                                                .GetMsTeamsBotApplicationInstallationIdForUserAsync(employeeOverTimeRecord.EmployeeId);
                if (string.IsNullOrEmpty(userMsTeamsBotAppInstallationId))
                {
                    await _msGraphSdkClientService.InstallMsTeamsBotApplicationForUserAsync(employeeOverTimeRecord.EmployeeId);
                }

                var botMessage = new BotMessage
                {
                    UserId = employeeOverTimeRecord.EmployeeId,
                    Content = $"Dear {employeeOverTimeRecord.FirstName}," +
                                            $" you work too much. You total working hours from the last week equals {employeeOverTimeRecord.TotalWorkingHoursPerWeek}." +
                                            $" Please get some rest."
                };
                var dataAsString = JsonSerializer.Serialize(botMessage);
                var content = new StringContent(dataAsString);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                await _httpClient.PostAsync(_botEndpointConfiguration.NotificationEndpointUrl, content);
            }
        }
    }
}
