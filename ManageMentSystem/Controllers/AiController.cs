using Google.GenAI.Types;
using ManageMentSystem.Services.AiServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        private readonly IAiOrchestratorService _orchestrator;
        private const string HistorySessionKey = "ai_chat_history";

        public AiController(IAiOrchestratorService orchestrator)
        {
            _orchestrator = orchestrator;
        }

        // GET /Ai  → صفحة الـ Chat
        public IActionResult Index()
        {
            return View();
        }

        // POST /Ai/Chat → رد كامل JSON
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { error = "الرسالة فارغة" });

            var history = GetOrCreateHistory();

            try
            {
                var reply = await _orchestrator.ChatAsync(history, request.Message);
                SaveHistory(history);
                return Ok(new { reply });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "حدث خطأ في الاتصال بالذكاء الاصطناعي", details = ex.Message });
            }
        }

        // POST /Ai/Stream → SSE Streaming حرف بحرف
        [HttpPost]
        public async Task Stream([FromBody] ChatRequest request)
        {
            Response.Headers["Content-Type"]  = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                await Response.WriteAsync("data: [ERROR] الرسالة فارغة\n\n");
                return;
            }

            var history = GetOrCreateHistory();

            try
            {
                await foreach (var chunk in _orchestrator.StreamAsync(history, request.Message))
                {
                    // Escape JSON special chars for safe SSE transport
                    var safeChunk = chunk.Replace("\n", "\\n").Replace("\r", "");
                    await Response.WriteAsync($"data: {safeChunk}\n\n");
                    await Response.Body.FlushAsync();
                }

                SaveHistory(history);
                await Response.WriteAsync("data: [DONE]\n\n");
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                //if (errorMsg.Contains("quota", StringComparison.OrdinalIgnoreCase) || errorMsg.Contains("429"))
                //{
                //    errorMsg = "عذراً، لقد تجاوزت الحد المجاني المسموح به للطلبات. يرجى الانتظار دقيقة والمحاولة مجدداً.";
                //}
                
                await Response.WriteAsync($"data: [ERROR] {errorMsg}\n\n");
                await Response.Body.FlushAsync();
            }
        }

        // GET /Ai/GetHistory → استرجاع السجل الحالي
        [HttpGet]
        public IActionResult GetHistory()
        {
            var history = GetOrCreateHistory();
            // تصفية السجل لبعث النصوص فقط (المستخدم والموديل) وتجاهل الـ function calls/responses للتبسيط في العرض
            var simpleHistory = history
                .Where(c => c.Role == "user" || c.Role == "model")
                .Select(c => new
                {
                    role = c.Role,
                    text = string.Join("\n", c.Parts.Select(p => p.Text))
                })
                .Where(c => !string.IsNullOrEmpty(c.text))
                .ToList();

            return Ok(simpleHistory);
        }

        // POST /Ai/Clear → مسح سجل المحادثة
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(HistorySessionKey);
            return Ok(new { message = "تم مسح المحادثة" });
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private List<Content> GetOrCreateHistory()
        {
            var json = HttpContext.Session.GetString(HistorySessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<Content>();

            try
            {
                return JsonSerializer.Deserialize<List<Content>>(json) ?? new List<Content>();
            }
            catch
            {
                return new List<Content>();
            }
        }

        private void SaveHistory(List<Content> history)
        {
            // احتفظ بآخر 20 رسالة فقط لتفادي session overflow
            if (history.Count > 20)
                history = history.Skip(history.Count - 20).ToList();

            try
            {
                var json = JsonSerializer.Serialize(history);
                HttpContext.Session.SetString(HistorySessionKey, json);
            }
            catch
            {
                // ignore serialization errors
            }
        }
    }

    public class ChatRequest
    {
        public string? Message { get; set; }
    }
}
