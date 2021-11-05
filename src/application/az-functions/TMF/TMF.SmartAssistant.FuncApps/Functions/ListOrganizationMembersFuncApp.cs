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
    internal class ListOrganizationMembersFuncApp
    {
        private readonly IMsGraphSdkClientService _msGraphSdkClientService;

        public ListOrganizationMembersFuncApp(IMsGraphSdkClientService msGraphSdkClientService)
        {
            _msGraphSdkClientService = msGraphSdkClientService
                                                ?? throw new ArgumentNullException(nameof(msGraphSdkClientService));
        }

        [FunctionName(FunctionNamesRepository.ListOrganizationMembersFuncAppName)]
        public async Task<List<OrganizationMember>> ListOrganizationMembersAsync([ActivityTrigger] object param)
        {
            var allMembers = await _msGraphSdkClientService.GetAllUsersAsync();
            return allMembers;
        }
    }
}
