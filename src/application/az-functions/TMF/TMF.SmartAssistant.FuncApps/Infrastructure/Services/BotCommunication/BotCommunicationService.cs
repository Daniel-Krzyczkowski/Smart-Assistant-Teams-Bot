using System;
using System.Threading.Tasks;
using TMF.SmartAssistant.FuncApps.Application.Model;

namespace TMF.SmartAssistant.FuncApps.Infrastructure.Services.BotCommunication
{
    internal interface IBotCommunicationService
    {
        Task SendMessageToAzureBotAsync(BotMessage botMessage);
    }

    internal class BotCommunicationService : IBotCommunicationService
    {
        public Task SendMessageToAzureBotAsync(BotMessage botMessage)
        {
            throw new NotImplementedException();
        }
    }
}
