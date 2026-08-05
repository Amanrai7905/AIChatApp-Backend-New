namespace AIChatApp.DTOs
{
    public class ChatRequestDto
    {
        public Guid ConversationId { get; set; }
        public string Prompt { get; set; }
    }
}
