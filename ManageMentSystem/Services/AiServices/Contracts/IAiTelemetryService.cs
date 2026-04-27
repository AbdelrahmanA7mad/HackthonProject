namespace ManageMentSystem.Services.AiServices
{
    public interface IAiTelemetryService
    {
        void TrackStatusTransition(string status);
        void TrackToolExecution(string functionName, bool success, int attempt, long durationMs, string? errorCode = null);
        void TrackStreamCompletion(bool success, long durationMs, int outputChars);
        void TrackError(string scope, string code);
        object GetSnapshot();
    }
}

