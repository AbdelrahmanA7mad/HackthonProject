using OpenRouter.NET.Models;

namespace ManageMentSystem.Services.AiServices
{
    /// <summary>
    /// Interface الرئيسي لخدمة الـ AI Orchestrator
    /// </summary>
    public interface IAiOrchestratorService
    {
        /// <summary>رد كامل بعد انتهاء المعالجة</summary>
        Task<string> ChatAsync(List<Message> history, string userMessage);

        /// <summary>رد streaming حرف بحرف للـ SSE</summary>
        IAsyncEnumerable<string> StreamAsync(List<Message> history, string userMessage, CancellationToken cancellationToken = default);
    }
}
