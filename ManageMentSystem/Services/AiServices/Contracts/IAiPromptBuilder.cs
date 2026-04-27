namespace ManageMentSystem.Services.AiServices
{
    public interface IAiPromptBuilder
    {
        Task<string> BuildSystemPromptAsync();
    }
}

