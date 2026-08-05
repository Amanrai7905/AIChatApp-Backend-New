namespace AIChatApp.Models
{
    public class Conversation
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public Guid UserId { get; set; }

        public AppUser User { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<ChatMessage> Messages { get; set; }
    }
}
