using OpenRouter.NET;
using OpenRouter.NET.Models;
using System.Runtime.CompilerServices;
using System.Text;

namespace ManageMentSystem.Services.AiServices
{
    /// <summary>
    /// المحرك الرئيسي: يدير الحوار مع OpenRouter ويدير دورة الـ Tool Calling الكاملة
    /// </summary>
    public class AiOrchestratorService : IAiOrchestratorService
    {
        private readonly OpenRouterClient _client;
        private readonly IConfiguration _config;

        private const string SystemPrompt =
            "أنت مساعد تجاري ذكي لنظام نقاط البيع \"قطة\".\n" +
            "مهمتك: تجيب على أسئلة صاحب المتجر بدقة من البيانات الفعلية.\n" +
            "القواعد:\n" +
            "- دايمًا استخدم الـ tools للحصول على الأرقام والبيانات، لا تخمن أبداً\n" +
            "- رد بالعربية بشكل واضح ومختصر ومنظم\n" +
            "- استخدم أرقام دقيقة من الـ tools، ولا تقول تقريباً\n" +
            "- لو السؤال مش متعلق بالأعمال، وضّح بأدب أنك متخصص في بيانات المتجر\n" +
            "- لما تعرض قوائم، استخدم نقاط أو ترقيم لسهولة القراءة";

        public AiOrchestratorService(OpenRouterClient client, IConfiguration config)
        {
            _client = client;
            _config = config;

            // تسجيل كل الـ Tools
            _client.RegisterTool<GetTotalSalesTool>();
            _client.RegisterTool<GetTopProductsTool>();
            _client.RegisterTool<GetMonthlySalesTool>();
            _client.RegisterTool<GetProfitTool>();
            _client.RegisterTool<GetLowStockProductsTool>();
            _client.RegisterTool<GetTopCustomersTool>();
            _client.RegisterTool<GetStoreAccountSummaryTool>();
            _client.RegisterTool<GetPendingDebtsTool>();
            _client.RegisterTool<GetGeneralStatisticsTool>();
        }

        public async Task<string> ChatAsync(List<Message> history, string userMessage)
        {
            var model = _config["OpenRouter:Model"] ?? "anthropic/claude-3-haiku-20240307";

            history.Add(Message.FromUser(userMessage));

            var request = new ChatCompletionRequest
            {
                Model = model,
                Messages = history
            };

            var response = await _client.CreateChatCompletionAsync(request);
            var text = response.Choices?[0]?.Message?.Content?.ToString() ?? "";

            history.Add(Message.FromAssistant(text));
            return text;
        }

        public async IAsyncEnumerable<string> StreamAsync(
            List<Message> history,
            string userMessage,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var model = _config["OpenRouter:Model"] ?? "anthropic/claude-3-haiku-20240307";

            history.Add(Message.FromUser(userMessage));

            var request = new ChatCompletionRequest
            {
                Model = model,
                Messages = history
            };

            var responseBuilder = new StringBuilder();

            await foreach (var chunk in _client.StreamAsync(request).WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested) break;

                if (chunk.TextDelta != null)
                {
                    responseBuilder.Append(chunk.TextDelta);
                    yield return chunk.TextDelta;
                }
            }

            history.Add(Message.FromAssistant(responseBuilder.ToString()));
        }
    }
}
