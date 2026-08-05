using AIChatApp.Data;
using AIChatApp.DTOs;
using AIChatApp.Models;
using AIChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIChatApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly GroqService _groqService;
        private readonly AppDbContext _context;

        public ChatController(GroqService groqService, AppDbContext context)
        {
            _groqService = groqService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AskQuestion(ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace( request.Prompt))
            {
                return BadRequest("Prompt is required");
            }

            var conversation = await _context.Conversations.FirstOrDefaultAsync(x => x.Id == request.ConversationId);
            if (conversation == null)
            {
                return BadRequest("Conversation not found");
            }

            // LOAD LAST 5 MESSAGES
            var oldMessages = await _context.ChatMessages.Where(x => x.ConversationId == request.ConversationId)
                .OrderBy(x => x.CreatedAt)
                .Take(5)
                .ToListAsync();

            // BUILD AI CONTEXT
            var fullPrompt = @"You are a helpful AI assistant.Answer professionally and clearly.";

            foreach (var msg in oldMessages)
            {
                fullPrompt += $"User: {msg.UserMessage}\n";

                fullPrompt += $"Assistant: {msg.AIResponse}\n";
            }

            // CURRENT QUESTION
            fullPrompt += $"User: {request.Prompt}\n";

            fullPrompt += "Assistant:";

            // ASK AI
            var aiResponse = await _groqService.AskAI(fullPrompt);

            // SAVE MESSAGE
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = request.ConversationId,
                UserMessage = request.Prompt,
                AIResponse = aiResponse,
                CreatedAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new{response = aiResponse });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var chats = await _context.ChatMessages
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(chats);
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetMessages(Guid conversationId)
        {
            var messages = await _context.ChatMessages.Where(x => x.ConversationId == conversationId).OrderBy(x => x.CreatedAt).ToListAsync();
            return Ok(messages);
        }


    }
}
