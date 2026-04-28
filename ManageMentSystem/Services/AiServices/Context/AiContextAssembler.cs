using OpenRouter.NET.Models;

namespace ManageMentSystem.Services.AiServices
{
    public class AiContextAssembler : IAiContextAssembler
    {
        private readonly IAiConversationService _conversationService;
        private readonly IAiPromptBuilder _promptBuilder;
        private readonly IConfiguration _configuration;

        public AiContextAssembler(
            IAiConversationService conversationService,
            IAiPromptBuilder promptBuilder,
            IConfiguration configuration)
        {
            _conversationService = conversationService;
            _promptBuilder = promptBuilder;
            _configuration = configuration;
        }

        public async Task<List<Message>> BuildHistoryAsync(int? conversationId, CancellationToken cancellationToken = default)
        {
            var maxHistoryMessages = ParsePositiveInt(_configuration["AI:Context:MaxHistoryMessages"], 10);

            var history = new List<Message>
            {
                Message.FromSystem(await _promptBuilder.BuildSystemPromptAsync())
            };

            if (!conversationId.HasValue || conversationId.Value <= 0)
            {
                return history;
            }

            var conv = await _conversationService.GetConversationAsync(conversationId.Value);
            if (conv == null)
            {
                return history;
            }

            var messages = conv.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Take(maxHistoryMessages)
                .OrderBy(m => m.CreatedAt);

            foreach (var message in messages)
            {
                if (message.Role == "user")
                {
                    history.Add(Message.FromUser(message.Content));
                }
                else
                {
                    history.Add(Message.FromAssistant(message.Content));
                }
            }

            return history;
        }

        private static int ParsePositiveInt(string? value, int fallback)
        {
            if (int.TryParse(value, out var parsed) && parsed > 0)
            {
                return parsed;
            }

            return fallback;
        }
    }
}

