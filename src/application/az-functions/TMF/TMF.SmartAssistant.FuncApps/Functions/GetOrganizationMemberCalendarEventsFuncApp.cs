using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMF.SmartAssistant.FuncApps.Application.Model;
using TMF.SmartAssistant.FuncApps.Infrastructure.Services.MsGraph;
using TMF.SmartAssistant.FuncApps.Utils;

namespace TMF.SmartAssistant.FuncApps.Functions
{
    internal class GetOrganizationMemberCalendarEventsFuncApp
    {
        private readonly IMsGraphSdkClientService _msGraphSdkClientService;

        public GetOrganizationMemberCalendarEventsFuncApp(IMsGraphSdkClientService msGraphSdkClientService)
        {
            _msGraphSdkClientService = msGraphSdkClientService
                                                    ?? throw new ArgumentNullException(nameof(msGraphSdkClientService));
        }

        [FunctionName(FunctionNamesRepository.GetOrganizationMemberCalendarEventsFuncAppName)]
        public async Task<List<CalendarEvent>> GetOrganizationMemberCalendarEventsAsync([ActivityTrigger] IReadOnlyList<OrganizationMember> organizationMembers)
        {
            List<CalendarEvent> organizationMembersCalendarEvents = new List<CalendarEvent>();

            foreach (var member in organizationMembers)
            {
                var calendarEvents = await _msGraphSdkClientService.GetUserCalendarEventsFromThePastWeekForUserAsync(member.Id);
                if (calendarEvents.Count != 0)
                {
                    organizationMembersCalendarEvents.AddRange(calendarEvents);
                }
            }

            return organizationMembersCalendarEvents;
        }
    }
}
