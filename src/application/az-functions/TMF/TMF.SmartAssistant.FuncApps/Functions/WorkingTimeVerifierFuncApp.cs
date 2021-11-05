using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMF.SmartAssistant.FuncApps.Application.Model;
using TMF.SmartAssistant.FuncApps.Infrastructure.Data;
using TMF.SmartAssistant.FuncApps.Utils;

namespace TMF.SmartAssistant.FuncApps.Functions
{
    internal class WorkingTimeVerifierFuncApp
    {
        private readonly IOrganizationMemberOverTimeRecordRepository _employeeOverTimeRecordRepository;
        private readonly List<EmployeeOverTimeRecord> _organizationMembersWithExtendedWorkingTime;

        public WorkingTimeVerifierFuncApp(IOrganizationMemberOverTimeRecordRepository employeeOverTimeRecordRepository)
        {
            _employeeOverTimeRecordRepository = employeeOverTimeRecordRepository
                                                    ?? throw new ArgumentNullException(nameof(employeeOverTimeRecordRepository));
            _organizationMembersWithExtendedWorkingTime = new List<EmployeeOverTimeRecord>();
        }

        [FunctionName(FunctionNamesRepository.WorkingTimeVerifierFuncAppName)]
        public async Task<IList<EmployeeOverTimeRecord>> VerifyWorkingTime([ActivityTrigger] IDurableActivityContext inputs)
        {
            var (organizationMembersCalendarEvents, organizationMembers) = inputs.GetInput<(IReadOnlyList<CalendarEvent>, List<OrganizationMember>)>();

            var groupedCalendarEvents = organizationMembersCalendarEvents.GroupBy(e => e.OwnerId).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

            foreach (var organizationMemberId in groupedCalendarEvents.Keys)
            {
                var calendarEventsForMember = groupedCalendarEvents[organizationMemberId];
                double totalWorkingTimeInHours = 0;

                foreach (var calendarEvent in calendarEventsForMember)
                {
                    var totalHoursPerDay = (calendarEvent.EndTime - calendarEvent.StartTime).TotalHours;
                    totalWorkingTimeInHours += totalHoursPerDay;
                }

                if (totalWorkingTimeInHours > 40)
                {
                    var organizationMemberWithExtendedWorkingTime = organizationMembers.First(m => m.Id == organizationMemberId);
                    await AddEmployeeOverTimeRecordAsync(organizationMemberWithExtendedWorkingTime, totalWorkingTimeInHours);
                }
            }

            return _organizationMembersWithExtendedWorkingTime;
        }

        private async Task AddEmployeeOverTimeRecordAsync(OrganizationMember organizationMember,
                                                          double totalWorkingHours)
        {
            var employeeOverTimeRecord = new EmployeeOverTimeRecord
            {
                Id = Guid.NewGuid().ToString(),
                EmployeeId = organizationMember.Id,
                FirstName = organizationMember.FirstName,
                LastName = organizationMember.LastName,
                TotalWorkingHoursPerWeek = totalWorkingHours
            };

            await _employeeOverTimeRecordRepository.AddAsync(employeeOverTimeRecord);
            _organizationMembersWithExtendedWorkingTime.Add(employeeOverTimeRecord);
        }
    }
}
