using OpenRouter.NET.Models;

namespace ManageMentSystem.Services.AiServices
{
    public interface IAiContextAssembler
    {
        Task<List<Message>> BuildHistoryAsync(int? conversationId, CancellationToken cancellationToken = default);
    }
}

