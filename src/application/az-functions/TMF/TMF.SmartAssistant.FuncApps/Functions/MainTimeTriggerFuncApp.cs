using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TMF.SmartAssistant.FuncApps.Utils;

namespace TMF.SmartAssistant.FuncApps.Functions
{
    internal class MainTimeTriggerFuncApp
    {
        [FunctionName(FunctionNamesRepository.MainTimeTriggerFuncAppName)]
        public async Task HttpStart([TimerTrigger("0 */1 * * * *")] TimerInfo timerInfo,
                                    [DurableClient] IDurableOrchestrationClient starter,
                                    ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync(FunctionNamesRepository.UserTimeProcessingOrchestratorName, null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }
    }
}
