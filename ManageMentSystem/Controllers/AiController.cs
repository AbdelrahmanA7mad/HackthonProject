using ManageMentSystem.Services.AiServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        private readonly IAiOrchestratorService _orchestrator;
        private readonly IAiConversationService _conversationService;
        private readonly IAiContextAssembler _contextAssembler;
        private readonly IAiTelemetryService _telemetry;

        public AiController(
            IAiOrchestratorService orchestrator,
            IAiConversationService conversationService,
            IAiContextAssembler contextAssembler,
            IAiTelemetryService telemetry)
        {
            _orchestrator = orchestrator;
            _conversationService = conversationService;
            _contextAssembler = contextAssembler;
            _telemetry = telemetry;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Telemetry()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var list = await _conversationService.GetUserConversationsAsync();
            return Ok(list.Select(c => new { id = c.Id, title = c.Title, updatedAt = c.UpdatedAt }));
        }

        [HttpGet]
        public async Task<IActionResult> GetConversation(int id)
        {
            var conv = await _conversationService.GetConversationAsync(id);
            if (conv == null)
            {
                return NotFound();
            }

            var messages = conv.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    role = m.Role,
                    text = m.Content
                });

            return Ok(messages);
        }

        // Used by the floating widget in _AiChatWidget.cshtml
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var list = await _conversationService.GetRecentMessagesAsync(50);
            return Ok(list.Select(m => new { role = m.Role, text = m.Content }));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteConversation(int id)
        {
            await _conversationService.DeleteConversationAsync(id);
            return Ok(new { success = true });
        }

        // Used by the floating widget in _AiChatWidget.cshtml
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            await _conversationService.ClearUserConversationsAsync();
            return Ok(new { success = true });
        }

        // Diagnostic endpoint for internal AI telemetry counters
        [HttpGet]
        public IActionResult TelemetrySnapshot()
        {
            return Ok(_telemetry.GetSnapshot());
        }

        [HttpPost]
        public async Task Stream([FromBody] ChatRequest request)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                await Response.WriteAsync("data: [ERROR] الرسالة فارغة.\n\n");
                return;
            }

            int currentConversationId = request.ConversationId ?? 0;
            var history = await _contextAssembler.BuildHistoryAsync(currentConversationId);

            if (currentConversationId > 0)
            {
                var conv = await _conversationService.GetConversationAsync(currentConversationId);
                if (conv == null)
                {
                    await Response.WriteAsync("data: [ERROR] لم يتم العثور على المحادثة.\n\n");
                    return;
                }
            }
            else
            {
                string title = request.Message.Length > 30
                    ? request.Message[..30] + "..."
                    : request.Message;

                var newConv = await _conversationService.CreateConversationAsync(title);
                currentConversationId = newConv.Id;

                await Response.WriteAsync($"data: [CONVERSATION_ID] {currentConversationId}\n\n");
                await Response.Body.FlushAsync();
            }

            await _conversationService.AddMessageAsync(currentConversationId, "user", request.Message);

            try
            {
                var fullModelResponse = new StringBuilder();

                await foreach (var chunk in _orchestrator.StreamAsync(history, request.Message))
                {
                    var safeChunk = chunk.Replace("\n", "\\n").Replace("\r", string.Empty);
                    await Response.WriteAsync($"data: {safeChunk}\n\n");
                    await Response.Body.FlushAsync();

                    if (!chunk.StartsWith("[STATUS]") && !chunk.StartsWith("[TOOL]"))
                    {
                        fullModelResponse.Append(chunk);
                    }
                }

                var finalText = fullModelResponse.ToString();
                if (!string.IsNullOrWhiteSpace(finalText))
                {
                    await _conversationService.AddMessageAsync(currentConversationId, "assistant", finalText);
                }

                await Response.WriteAsync("data: [DONE]\n\n");
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                _telemetry.TrackError("controller_stream", "exception");
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

