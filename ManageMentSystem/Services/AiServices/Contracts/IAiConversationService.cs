using ManageMentSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManageMentSystem.Services.AiServices
{
    public interface IAiConversationService
    {
        Task<AiConversation> CreateConversationAsync(string title);
        Task<AiConversation?> GetConversationAsync(int conversationId);
        Task<List<AiConversation>> GetUserConversationsAsync();
        Task<List<AiMessage>> GetRecentMessagesAsync(int count = 50);
        Task AddMessageAsync(int conversationId, string role, string content);
        Task DeleteConversationAsync(int conversationId);
        Task ClearUserConversationsAsync();
    }
}
