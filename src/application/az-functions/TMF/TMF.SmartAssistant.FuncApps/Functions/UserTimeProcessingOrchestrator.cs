using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMF.SmartAssistant.FuncApps.Application.Model;
using TMF.SmartAssistant.FuncApps.Utils;

namespace TMF.SmartAssistant.FuncApps.Functions
{
    public class UserTimeProcessingOrchestrator
    {
        [FunctionName(FunctionNamesRepository.UserTimeProcessingOrchestratorName)]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            List<OrganizationMember> organizationMembers = await context
                                                                    .CallActivityAsync<List<OrganizationMember>>(FunctionNamesRepository.ListOrganizationMembersFuncAppName,
                                                                                                                                                                                    null);

            List<CalendarEvent> calendarEvents = await context.CallActivityAsync<List<CalendarEvent>>(FunctionNamesRepository.GetOrganizationMemberCalendarEventsFuncAppName,
                                                                                                                                                            organizationMembers);

            List<EmployeeOverTimeRecord> organizationMembersWithExtendedWorkingTime = await context.CallActivityAsync<List<EmployeeOverTimeRecord>>(FunctionNamesRepository.WorkingTimeVerifierFuncAppName,
                                                                                                                                                              (calendarEvents, organizationMembers));

            if (organizationMembersWithExtendedWorkingTime.Count > 0)
            {
                await context.CallActivityAsync(FunctionNamesRepository.BotMessageSenderFuncAppName, organizationMembersWithExtendedWorkingTime);
            }
        }
    }
}