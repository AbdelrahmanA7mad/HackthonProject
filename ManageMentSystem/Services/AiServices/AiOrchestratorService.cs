using Google.GenAI;
using Google.GenAI.Types;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace ManageMentSystem.Services.AiServices
{
    /// <summary>
    /// المحرك الرئيسي: يدير الحوار مع Gemini ويدير دورة الـ Function Calling الكاملة
    /// </summary>
    public class AiOrchestratorService : IAiOrchestratorService
    {
        private readonly Client _client;
        private readonly IAiToolExecutor _toolExecutor;
        private readonly IConfiguration _config;
        private readonly Tool _tools;

        private const string SystemPrompt =
            "أنت مساعد تجاري ذكي لنظام نقاط البيع \"قطة\".\n" +
            "مهمتك: تجيب على أسئلة صاحب المتجر بدقة من البيانات الفعلية.\n" +
            "القواعد:\n" +
            "- دايمًا استخدم الـ tools للحصول على الأرقام والبيانات، لا تخمن أبداً\n" +
            "- رد بالعربية بشكل واضح ومختصر ومنظم\n" +
            "- استخدم أرقام دقيقة من الـ tools، ولا تقول تقريباً\n" +
            "- لو السؤال مش متعلق بالأعمال، وضّح بأدب أنك متخصص في بيانات المتجر\n" +
            "- لما تعرض قوائم، استخدم نقاط أو ترقيم لسهولة القراءة";

        public AiOrchestratorService(Client client, IAiToolExecutor toolExecutor, IConfiguration config)
        {
            _client       = client;
            _toolExecutor = toolExecutor;
            _config       = config;
            _tools        = AiToolDefinitions.BuildTools();
        }

        // ─── رد كامل (بدون streaming) ─────────────────────────────────────────

        public async Task<string> ChatAsync(List<Content> history, string userMessage)
        {
            // 1. إضافة رسالة المستخدم
            history.Add(new Content
            {
                Role  = "user",
                Parts = new List<Part> { new Part { Text = userMessage } }
            });

            var genConfig = BuildConfig();
            var model = _config["Gemini:Model"] 
                ?? throw new InvalidOperationException("إعداد Gemini:Model غير موجود في ملف appsettings.json");

            // 2. إرسال للـ Gemini
            var response = await _client.Models.GenerateContentAsync(
                model:    model,
                contents: history,
                config:   genConfig
            );

            // 3. تحقق من Function Call
            var functionCall = ExtractFunctionCall(response);

            if (functionCall != null)
            {
                // 4. تنفيذ الـ function محلياً
                var functionResult = await _toolExecutor.ExecuteAsync(
                    functionCall.Name,
                    functionCall.Args ?? new Dictionary<string, object>()
                );

                var dictResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(functionResult));

                // 5. إضافة model turn + function response للـ history
                history.Add(response.Candidates![0].Content!);
                history.Add(new Content
                {
                    Role  = "user",
                    Parts = new List<Part>
                    {
                        Part.FromFunctionResponse(
                            name:     functionCall.Name,
                            response: dictResponse
                        )
                    }
                });

                // 6. استدعاء Gemini للرد النهائي
                var finalResponse = await _client.Models.GenerateContentAsync(
                    model:    model,
                    contents: history,
                    config:   genConfig
                );

                var finalText = finalResponse.Text ?? "";
                history.Add(new Content
                {
                    Role  = "model",
                    Parts = new List<Part> { new Part { Text = finalText } }
                });
                return finalText;
            }

            // رد مباشر بدون function call
            var directText = response.Text ?? "";
            history.Add(new Content
            {
                Role  = "model",
                Parts = new List<Part> { new Part { Text = directText } }
            });
            return directText;
        }

        // ─── Streaming SSE ────────────────────────────────────────────────────

        public async IAsyncEnumerable<string> StreamAsync(
            List<Content> history,
            string userMessage,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // 1. إضافة رسالة المستخدم
            history.Add(new Content
            {
                Role  = "user",
                Parts = new List<Part> { new Part { Text = userMessage } }
            });

            var genConfig = BuildConfig();
            var model = _config["Gemini:Model"] 
                ?? throw new InvalidOperationException("إعداد Gemini:Model غير موجود في ملف appsettings.json");

            // 2. أول استدعاء — قد يكون function call
            var response = await _client.Models.GenerateContentAsync(
                model:    model,
                contents: history,
                config:   genConfig
            );

            var functionCall = ExtractFunctionCall(response);

            if (functionCall != null)
            {
                // إشعار للـ UI إن الـ AI بيجمع البيانات
                yield return "⚙️ جاري تحليل البيانات...";

                var functionResult = await _toolExecutor.ExecuteAsync(
                    functionCall.Name,
                    functionCall.Args ?? new Dictionary<string, object>()
                );

                var dictResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(functionResult));

                history.Add(response.Candidates![0].Content!);
                history.Add(new Content
                {
                    Role  = "user",
                    Parts = new List<Part>
                    {
                        Part.FromFunctionResponse(
                            name:     functionCall.Name,
                            response: dictResponse
                        )
                    }
                });

                // streaming للرد النهائي
                var responseBuilder = new StringBuilder();
                await foreach (var chunk in _client.Models.GenerateContentStreamAsync(
                    model:    model,
                    contents: history,
                    config:   genConfig))
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    var text = chunk.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        responseBuilder.Append(text);
                        yield return text;
                    }
                }

                history.Add(new Content
                {
                    Role  = "model",
                    Parts = new List<Part> { new Part { Text = responseBuilder.ToString() } }
                });
            }
            else
            {
                // رد مباشر بدون function call — stream مباشرة
                var responseBuilder = new StringBuilder();
                await foreach (var chunk in _client.Models.GenerateContentStreamAsync(
                    model:    model,
                    contents: new List<Content>(history),
                    config:   genConfig))
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    var text = chunk.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        responseBuilder.Append(text);
                        yield return text;
                    }
                }

                history.Add(new Content
                {
                    Role  = "model",
                    Parts = new List<Part> { new Part { Text = responseBuilder.ToString() } }
                });
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private GenerateContentConfig BuildConfig() => new GenerateContentConfig
        {
            SystemInstruction = new Content { Role = "system", Parts = new List<Part> { new Part { Text = SystemPrompt } } },
            Tools             = new List<Tool> { _tools }
        };

        private static FunctionCall? ExtractFunctionCall(GenerateContentResponse response)
        {
            return response.Candidates?
                .SelectMany(c => c.Content?.Parts ?? new List<Part>())
                .FirstOrDefault(p => p.FunctionCall != null)
                ?.FunctionCall;
        }
    }
}
