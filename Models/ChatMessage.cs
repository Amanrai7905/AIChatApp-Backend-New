namespace AIChatApp.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }

        public Guid ConversationId { get; set; }

        public Conversation Conversation { get; set; }

        public string UserMessage { get; set; }

        public string AIResponse { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
