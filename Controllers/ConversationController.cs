using AIChatApp.Data;
using AIChatApp.DTOs;
using AIChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIChatApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConversationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateConversation(CreateConversationDto request)
        {
            var userId =User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid token");
            }
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                UserId = Guid.Parse(userId),
                CreatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);

            await _context.SaveChangesAsync();

            return Ok(conversation);
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var userId =User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

            var conversations = await _context.Conversations
                .Where(x => x.UserId == Guid.Parse(userId))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(conversations);
        }

        [AllowAnonymous]
        [HttpGet("debug")]
        public IActionResult Debug()
        {
            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Claims = User.Claims.Select(x => new
                {
                    x.Type,
                    x.Value
                })
            });
        }

        // DELETE CONVERSATION
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConversation(Guid id)
        {
            var conversation =
                await _context.Conversations
                .FirstOrDefaultAsync(x => x.Id == id);

            if (conversation == null)
            {
                return NotFound();
            }

            // DELETE CHAT MESSAGES FIRST
            var messages =
                await _context.ChatMessages
                .Where(x => x.ConversationId == id)
                .ToListAsync();

            _context.ChatMessages.RemoveRange(messages);

            // DELETE CONVERSATION
            _context.Conversations.Remove(conversation);

            await _context.SaveChangesAsync();

            return Ok("Conversation deleted");
        }

        // RENAME CONVERSATION
        [HttpPut("{id}")]
        public async Task<IActionResult> RenameConversation(Guid id,CreateConversationDto request)
        {
            var conversation =
                await _context.Conversations
                .FirstOrDefaultAsync(x => x.Id == id);

            if (conversation == null)
            {
                return NotFound();
            }

            conversation.Title = request.Title;

            await _context.SaveChangesAsync();

            return Ok(conversation);
        }


    }
}
