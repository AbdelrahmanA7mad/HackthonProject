using ManageMentSystem.Services.AiServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenRouter.NET.Models;

namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        private readonly IAiOrchestratorService _orchestrator;
        private readonly IAiConversationService _conversationService;

        public AiController(IAiOrchestratorService orchestrator, IAiConversationService conversationService)
        {
            _orchestrator = orchestrator;
            _conversationService = conversationService;
        }

        // GET /Ai  → صفحة الـ Chat
        public IActionResult Index()
        {
            return View();
        }

        // GET /Ai/GetConversations → استرجاع قائمة المحادثات
        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var list = await _conversationService.GetUserConversationsAsync();
            return Ok(list.Select(c => new { id = c.Id, title = c.Title, updatedAt = c.UpdatedAt }));
        }

        // GET /Ai/GetConversation/{id} → استرجاع رسائل محادثة معينة
        [HttpGet]
        public async Task<IActionResult> GetConversation(int id)
        {
            var conv = await _conversationService.GetConversationAsync(id);
            if (conv == null) return NotFound();

            var messages = conv.Messages.OrderBy(m => m.CreatedAt).Select(m => new
            {
                role = m.Role,
                text = m.Content
            });
            return Ok(messages);
        }

        // DELETE /Ai/DeleteConversation/{id}
        [HttpDelete]
        public async Task<IActionResult> DeleteConversation(int id)
        {
            await _conversationService.DeleteConversationAsync(id);
            return Ok(new { success = true });
        }

        // POST /Ai/Stream → SSE Streaming حرف بحرف
        [HttpPost]
        public async Task Stream([FromBody] ChatRequest request)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                await Response.WriteAsync("data: [ERROR] الرسالة فارغة\n\n");
                return;
            }

            int currentConversationId = request.ConversationId ?? 0;
            var history = new List<Message>();

            // إضافة الـ system prompt
            history.Add(Message.FromSystem(
                "أنت مساعد تجاري ذكي لنظام نقاط البيع \"قطة\". " +
                "مهمتك: تجيب على أسئلة صاحب المتجر بدقة من البيانات الفعلية. " +
                "رد بالعربية بشكل واضح ومختصر ومنظم."
            ));

            if (currentConversationId > 0)
            {
                var conv = await _conversationService.GetConversationAsync(currentConversationId);
                if (conv != null)
                {
                    foreach (var m in conv.Messages.OrderBy(msg => msg.CreatedAt))
                    {
                        if (m.Role == "user")
                            history.Add(Message.FromUser(m.Content));
                        else
                            history.Add(Message.FromAssistant(m.Content));
                    }
                }
            }
            else
            {
                string title = request.Message.Length > 30 ? request.Message.Substring(0, 30) + "..." : request.Message;
                var newConv = await _conversationService.CreateConversationAsync(title);
                currentConversationId = newConv.Id;

                await Response.WriteAsync($"data: [CONVERSATION_ID] {currentConversationId}\n\n");
                await Response.Body.FlushAsync();
            }

            await _conversationService.AddMessageAsync(currentConversationId, "user", request.Message);

            try
            {
                var fullModelResponse = new System.Text.StringBuilder();

                await foreach (var chunk in _orchestrator.StreamAsync(history, request.Message))
                {
                    var safeChunk = chunk.Replace("\n", "\\n").Replace("\r", "");
                    await Response.WriteAsync($"data: {safeChunk}\n\n");
                    await Response.Body.FlushAsync();

                    if (!chunk.StartsWith("⚙️"))
                    {
                        fullModelResponse.Append(chunk);
                    }
                }

                var finalText = fullModelResponse.ToString();
                if (!string.IsNullOrWhiteSpace(finalText))
                {
                    await _conversationService.AddMessageAsync(currentConversationId, "model", finalText);
                }

                await Response.WriteAsync("data: [DONE]\n\n");
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                await Response.WriteAsync($"data: [ERROR] {ex.Message}\n\n");
                await Response.Body.FlushAsync();
            }
        }

        public class ChatRequest
        {
            public string? Message { get; set; }
            public int? ConversationId { get; set; }
        }
    }
}
