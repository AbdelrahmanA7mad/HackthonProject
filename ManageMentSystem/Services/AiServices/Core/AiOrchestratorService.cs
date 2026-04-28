using OpenRouter.NET;
using OpenRouter.NET.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ManageMentSystem.Services.AiServices
{
    /// <summary>
    /// Main AI engine: streams model output and emits step-based status events.
    /// </summary>
    public class AiOrchestratorService : IAiOrchestratorService
    {
        private readonly OpenRouterClient _client;
        private readonly IConfiguration _config;
        private readonly IAiTelemetryService _telemetry;

        public AiOrchestratorService(OpenRouterClient client, IConfiguration config, IAiTelemetryService telemetry)
        {
            _client = client;
            _config = config;
            _telemetry = telemetry;

            _client.RegisterTool<GetTotalSalesTool>();
            _client.RegisterTool<GetTopProductsTool>();
            _client.RegisterTool<GetMonthlySalesTool>();
            _client.RegisterTool<GetProfitTool>();
            _client.RegisterTool<GetLowStockProductsTool>();
            _client.RegisterTool<GetTopCustomersTool>();
            _client.RegisterTool<GetStoreAccountSummaryTool>();
            _client.RegisterTool<GetPendingDebtsTool>();
            _client.RegisterTool<GetGeneralStatisticsTool>();
            _client.RegisterTool<GetSalesReportTool>();
            _client.RegisterTool<GetInventoryReportTool>();
            _client.RegisterTool<GetCustomerReportTool>();
            _client.RegisterTool<GetFinancialReportTool>();
            _client.RegisterTool<GetGeneralDebtReportTool>();
            _client.RegisterTool<GetCategoryPerformanceReportTool>();
            _client.RegisterTool<GetInstallmentsSummaryTool>();
            _client.RegisterTool<GetPaymentMethodsSummaryTool>();
            _client.RegisterTool<GetCustomerInfoTool>();
            _client.RegisterTool<SearchProductTool>();
            _client.RegisterTool<GetExpenseDetailsTool>();
            _client.RegisterTool<GetCustomerAccountStatementTool>();
            _client.RegisterTool<GetStoreTransactionsTool>();
            _client.RegisterTool<GetSalesDetailsTool>();
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
            var text = response.Choices?[0]?.Message?.Content?.ToString() ?? string.Empty;

            history.Add(Message.FromAssistant(text));
            return text;
        }

        public async IAsyncEnumerable<string> StreamAsync(
            List<Message> history,
            string userMessage,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var completedSuccessfully = false;
            var model = _config["OpenRouter:Model"] ?? "anthropic/claude-3-haiku-20240307";

            history.Add(Message.FromUser(userMessage));

            var request = new ChatCompletionRequest
            {
                Model = model,
                Messages = history
            };

            var responseBuilder = new StringBuilder();
            var startedGeneration = false;

            _telemetry.TrackStatusTransition("analyzing");
            yield return "[STATUS] analyzing";

            try
            {
                await foreach (var chunk in _client.StreamAsync(request).WithCancellation(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _telemetry.TrackError("stream", "cancelled");
                        break;
                    }

                    if (chunk.TextDelta != null)
                    {
                        if (!startedGeneration)
                        {
                            startedGeneration = true;
                            _telemetry.TrackStatusTransition("generating");
                            yield return "[STATUS] generating";
                        }

                        responseBuilder.Append(chunk.TextDelta);
                        yield return chunk.TextDelta;
                    }
                }

                _telemetry.TrackStatusTransition("finalizing");
                yield return "[STATUS] finalizing";
                history.Add(Message.FromAssistant(responseBuilder.ToString()));
                completedSuccessfully = true;
            }
            finally
            {
                stopwatch.Stop();

                if (!completedSuccessfully && !cancellationToken.IsCancellationRequested)
                {
                    _telemetry.TrackError("stream", "exception");
                }

                _telemetry.TrackStreamCompletion(completedSuccessfully, stopwatch.ElapsedMilliseconds, responseBuilder.Length);
            }
        }
    }
}
