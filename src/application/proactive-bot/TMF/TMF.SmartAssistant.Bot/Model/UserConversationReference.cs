using Microsoft.Bot.Schema;

namespace TMF.SmartAssistant.Bot.Model
{
    public class UserConversationReference
    {
        public string Id { get; set; }
        public ConversationReference ConversationReference { get; set; }
    }
}
