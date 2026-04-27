namespace ManageMentSystem.Services.AiServices
{
    /// <summary>
    /// Interface لتنفيذ الـ functions التي يطلبها Gemini
    /// </summary>
    public interface IAiToolExecutor
    {
        Task<object> ExecuteAsync(string functionName, IDictionary<string, object> args);
    }
}
