using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMF.SmartAssistant.Bot.Model;
using TMF.SmartAssistant.Bot.Repositories;

namespace TMF.SmartAssistant.Bot.Bots
{
    public class ProactiveBot : TeamsActivityHandler
    {
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private const string WelcomeMessage = "Hello, this is Smart Assistant Bot, nice to meet you!";
        private readonly IConversationReferenceRepository _conversationReferenceRepository;

        public ProactiveBot(ConcurrentDictionary<string, ConversationReference> conversationReferences,
                            IConversationReferenceRepository conversationReferenceRepository)
        {
            _conversationReferences = conversationReferences
                                            ?? throw new ArgumentNullException(nameof(conversationReferences));
            _conversationReferenceRepository = conversationReferenceRepository
                                                    ?? throw new ArgumentNullException(nameof(conversationReferenceRepository));
        }

        private async Task AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();

            var userConversationReference = new UserConversationReference
            {
                Id = conversationReference.User.AadObjectId,
                ConversationReference = conversationReference
            };
            _conversationReferences.AddOrUpdate(conversationReference.User.AadObjectId, conversationReference, (key, newValue) => conversationReference);
            var existingConversationReference = await _conversationReferenceRepository.GetAsync(userConversationReference.Id);

            if (existingConversationReference == null)
            {
                await _conversationReferenceRepository.AddAsync(userConversationReference);
            }

            else
            {
                existingConversationReference.ConversationReference = conversationReference;
                await _conversationReferenceRepository.UpdateAsync(existingConversationReference);
            }
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await AddConversationReference(turnContext.Activity as Activity);

            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext,
                                                                                              CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(WelcomeMessage), cancellationToken);
                }
            }
        }

        protected override async Task OnReactionsAddedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            var addedReaction = messageReactions[0];
            if (addedReaction.Type == MessageReactionTypes.Like)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"I am happy that you like my advice."), cancellationToken);
            }
            await base.OnReactionsAddedAsync(messageReactions, turnContext, cancellationToken);
        }

        protected override async Task OnReactionsRemovedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            var removedReaction = messageReactions[0];
            if (removedReaction.Type == MessageReactionTypes.Like)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"I am sad that my advice was not helpful."), cancellationToken);
            }
            await base.OnReactionsAddedAsync(messageReactions, turnContext, cancellationToken);
        }
    }
}
