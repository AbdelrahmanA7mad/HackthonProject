using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.UserServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageMentSystem.Services.AiServices
{
    public class AiConversationService : IAiConversationService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public AiConversationService(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<AiConversation> CreateConversationAsync(string title)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = _userService.GetUserId();

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("يجب تسجيل الدخول أولاً.");
            }

            var conversation = new AiConversation
            {
                TenantId = tenantId,
                UserId = userId,
                Title = title,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AiConversations.Add(conversation);
            await _context.SaveChangesAsync();

            return conversation;
        }

        public async Task<AiConversation?> GetConversationAsync(int conversationId)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = _userService.GetUserId();

            return await _context.AiConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.TenantId == tenantId && c.UserId == userId);
        }

        public async Task<List<AiConversation>> GetUserConversationsAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = _userService.GetUserId();

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
            {
                return new List<AiConversation>();
            }

            return await _context.AiConversations
                .Where(c => c.TenantId == tenantId && c.UserId == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task AddMessageAsync(int conversationId, string role, string content)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = _userService.GetUserId();

            var conversation = await _context.AiConversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.TenantId == tenantId && c.UserId == userId);

            if (conversation != null)
            {
                var message = new AiMessage
                {
                    AiConversationId = conversationId,
                    Role = role,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AiMessages.Add(message);
                
                conversation.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteConversationAsync(int conversationId)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = _userService.GetUserId();

            var conversation = await _context.AiConversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.TenantId == tenantId && c.UserId == userId);

            if (conversation != null)
            {
                _context.AiConversations.Remove(conversation);
                await _context.SaveChangesAsync();
            }
        }
    }
}
