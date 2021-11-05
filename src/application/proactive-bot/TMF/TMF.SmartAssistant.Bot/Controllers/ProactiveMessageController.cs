using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TMF.SmartAssistant.Bot.Model;
using TMF.SmartAssistant.Bot.Repositories;

namespace TMF.SmartAssistant.Bot.Controllers
{
    [Authorize(Policy = "BotAccessAuthorizationPolicy")]
    [Route("api/notify")]
    [ApiController]
    public class ProactiveMessageController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly IConversationReferenceRepository _conversationReferenceRepository;

        public ProactiveMessageController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration,
                                ConcurrentDictionary<string,
                                ConversationReference> conversationReferences,
                                IConversationReferenceRepository conversationReferenceRepository)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _conversationReferenceRepository = conversationReferenceRepository;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] BotMessage botMessage)
        {
            var existingConversationReference = await _conversationReferenceRepository.GetAsync(botMessage.UserId);

            if (existingConversationReference != null)
            {
                await ((AdapterWithErrorHandler)_adapter)
                                                    .ContinueConversationAsync(_appId, existingConversationReference.ConversationReference,
                                                    async (context, token) => await BotCallbackAsync(botMessage, context, token),
                                                    default);
            }

            if (botMessage != null)
            {
                foreach (var userId in _conversationReferences.Keys)
                {
                    if (userId == botMessage.UserId)
                    {
                        var conversationReference = _conversationReferences[userId];
                        await ((AdapterWithErrorHandler)_adapter)
                                                            .ContinueConversationAsync(_appId, conversationReference,
                                                            async (context, token) => await BotCallbackAsync(botMessage, context, token),
                                                            default);
                    }
                }

                return Ok();
            }

            return BadRequest();
        }

        private async Task BotCallbackAsync(BotMessage botMessage, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(botMessage.Content, cancellationToken: cancellationToken);
        }
    }
}
