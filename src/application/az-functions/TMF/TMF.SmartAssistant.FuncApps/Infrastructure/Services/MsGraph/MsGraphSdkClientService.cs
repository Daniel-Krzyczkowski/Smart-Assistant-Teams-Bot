using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMF.SmartAssistant.FuncApps.Application.Model;
using TMF.SmartAssistant.FuncApps.Configuration;
using TMF.SmartAssistant.FuncApps.Infrastructure.Logger;

namespace TMF.SmartAssistant.FuncApps.Infrastructure.Services.MsGraph
{
    internal interface IMsGraphSdkClientService
    {
        Task<List<OrganizationMember>> GetAllUsersAsync();
        Task<string> GetMsTeamsBotApplicationInstallationIdForUserAsync(string userId);
        Task InstallMsTeamsBotApplicationForUserAsync(string userId);
        Task UnInstallMsTeamsBotApplicationForUserAsync(string userId, string msTeamAppInstallationId);
        Task<List<CalendarEvent>> GetUserCalendarEventsFromThePastWeekForUserAsync(string userId);
    }

    internal class MsGraphSdkClientService : IMsGraphSdkClientService
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IMsTeamsAppConfiguration _msTeamsAppConfiguration;
        private readonly ILogger<MsGraphSdkClientService> _logger;

        public MsGraphSdkClientService(GraphServiceClient graphServiceClient,
                               IMsTeamsAppConfiguration msTeamsAppConfiguration,
                               ILogger<MsGraphSdkClientService> logger)
        {
            _graphServiceClient = graphServiceClient
                 ?? throw new ArgumentNullException(nameof(graphServiceClient));

            _msTeamsAppConfiguration = msTeamsAppConfiguration
                 ?? throw new ArgumentNullException(nameof(msTeamsAppConfiguration));

            _logger = logger
                 ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<OrganizationMember>> GetAllUsersAsync()
        {
            List<User> users = new List<User>();
            try
            {

                IGraphServiceUsersCollectionPage graphServiceUsersCollectionPage = await _graphServiceClient.Users
                                                                       .Request()
                                                                       .Select($"id," +
                                                                       $" userPrincipalName," +
                                                                       $" givenName," +
                                                                       $" surname")
                                                                       .Top(50)
                                                                       .GetAsync();

                var userPageIterator = PageIterator<User>.CreatePageIterator(_graphServiceClient,
                                                           graphServiceUsersCollectionPage,
                                                           entity => { users.Add(entity); return true; });

                await userPageIterator.IterateAsync();
            }

            catch (ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogError(LoggerEvents.MicrosoftGraphTooManyRequests, string.Concat(ex.Message, " ", ex.InnerException?.Message));
                    var retryAfter = ex.ResponseHeaders.RetryAfter.Delta.Value;
                    await Task.Delay(TimeSpan.FromSeconds(retryAfter.TotalSeconds));
                    await GetAllUsersAsync();
                }

                else
                {
                    throw;
                }
            }

            var organizationUsers = users
                                  .Select(u => new OrganizationMember
                                  {
                                      Id = u.Id,
                                      Email = u.UserPrincipalName,
                                      FirstName = u.GivenName,
                                      LastName = u.Surname
                                  })
                                  .ToList();

            return organizationUsers;
        }

        public async Task<string> GetMsTeamsBotApplicationInstallationIdForUserAsync(string userId)
        {
            var installedAppForSpecificUser = await _graphServiceClient.Users[userId]
                                                    .Teamwork
                                                    .InstalledApps
                                                    .Request()
                                                    .Filter($"teamsApp/id eq '{_msTeamsAppConfiguration.AppId}'")
                                                    .GetAsync();
            if (installedAppForSpecificUser.Count == 0)
            {
                return string.Empty;
            }

            return installedAppForSpecificUser[0].Id;
        }

        public async Task InstallMsTeamsBotApplicationForUserAsync(string userId)
        {
            await _graphServiceClient.Users[userId]
                                        .Teamwork
                                        .InstalledApps.Request().AddAsync(new UserScopeTeamsAppInstallation
                                        {
                                            AdditionalData = new Dictionary<string, object>
                                            {
                                                            {"teamsApp@odata.bind", $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{_msTeamsAppConfiguration.AppId}"}
                                            }
                                        });
        }

        public async Task UnInstallMsTeamsBotApplicationForUserAsync(string userId, string msTeamAppInstallationId)
        {

            await _graphServiceClient.Users[userId].Teamwork.InstalledApps[msTeamAppInstallationId]
                                               .Request()
                                               .DeleteAsync();
        }

        public async Task<List<CalendarEvent>> GetUserCalendarEventsFromThePastWeekForUserAsync(string userId)
        {
            List<Event> events = new List<Event>();
            DateTime currentDate = DateTime.UtcNow;
            DateTime sundayfLastWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek - 7);
            DateTime saturdayLastWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek - 1);

            try
            {
                var userMailboxSettings = await _graphServiceClient.Users[userId]
                                                     .Request()
                                                     .Select("MailboxSettings")
                                                     .GetAsync();

                IUserEventsCollectionPage userEventsCollectionPage = await _graphServiceClient.Users[userId].Events
                            .Request()
                            .Select("id, subject, start, end")
                            .Filter($"start/dateTime gt '{sundayfLastWeek}' and end/dateTime lt '{saturdayLastWeek}'")
                            .GetAsync();

                var eventPageIterator = PageIterator<Event>.CreatePageIterator(_graphServiceClient,
                                               userEventsCollectionPage,
                                               entity => { events.Add(entity); return true; });

                await eventPageIterator.IterateAsync();
            }

            catch (ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogError(LoggerEvents.MicrosoftGraphTooManyRequests, string.Concat(ex.Message, " ", ex.InnerException?.Message));
                    var retryAfter = ex.ResponseHeaders.RetryAfter.Delta.Value;
                    await Task.Delay(TimeSpan.FromSeconds(retryAfter.TotalSeconds));
                    await GetUserCalendarEventsFromThePastWeekForUserAsync(userId);
                }

                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (ex.Error.Code.Equals("MailboxNotEnabledForRESTAPI"))
                    {
                        _logger.LogError(LoggerEvents.MicrosoftGraphUserDoesNotHaveActiveMailbox, string.Concat(ex.Message, " ", ex.InnerException?.Message));
                    }
                }

                else
                {
                    throw;
                }
            }


            var userCalendarEvents = events
                  .Select(e => new CalendarEvent
                  {
                      Id = e.Id,
                      StartTime = DateTime.Parse(e.Start.DateTime),
                      EndTime = DateTime.Parse(e.End.DateTime),
                      Subject = e.Subject,
                      OwnerId = userId
                  })
                  .ToList();

            return userCalendarEvents;
        }
    }
}
