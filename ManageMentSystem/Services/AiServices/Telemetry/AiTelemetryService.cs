using System.Collections.Concurrent;

namespace ManageMentSystem.Services.AiServices
{
    public class AiTelemetryService : IAiTelemetryService
    {
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, long> _statusCounters = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, long> _toolCounters = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, long> _errorCounters = new(StringComparer.OrdinalIgnoreCase);

        private long _streamSuccessCount;
        private long _streamFailureCount;
        private long _totalStreamDurationMs;
        private long _totalStreamOutputChars;

        public AiTelemetryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void TrackStatusTransition(string status)
        {
            if (!IsEnabled()) return;
            _statusCounters.AddOrUpdate(status, 1, (_, current) => current + 1);
        }

        public void TrackToolExecution(string functionName, bool success, int attempt, long durationMs, string? errorCode = null)
        {
            if (!IsEnabled()) return;

            _toolCounters.AddOrUpdate($"tool:{functionName}:calls", 1, (_, current) => current + 1);
            _toolCounters.AddOrUpdate($"tool:{functionName}:attempt:{attempt}", 1, (_, current) => current + 1);
            _toolCounters.AddOrUpdate($"tool:{functionName}:{(success ? "success" : "failure")}", 1, (_, current) => current + 1);
            _toolCounters.AddOrUpdate($"tool:{functionName}:duration_ms_total", durationMs, (_, current) => current + durationMs);

            if (!string.IsNullOrWhiteSpace(errorCode))
            {
                _errorCounters.AddOrUpdate($"tool:{functionName}:{errorCode}", 1, (_, current) => current + 1);
            }
        }

        public void TrackStreamCompletion(bool success, long durationMs, int outputChars)
        {
            if (!IsEnabled()) return;

            if (success)
            {
                Interlocked.Increment(ref _streamSuccessCount);
            }
            else
            {
                Interlocked.Increment(ref _streamFailureCount);
            }

            Interlocked.Add(ref _totalStreamDurationMs, durationMs);
            Interlocked.Add(ref _totalStreamOutputChars, outputChars);
        }

        public void TrackError(string scope, string code)
        {
            if (!IsEnabled()) return;
            _errorCounters.AddOrUpdate($"{scope}:{code}", 1, (_, current) => current + 1);
        }

        public object GetSnapshot()
        {
            return new
            {
                enabled = IsEnabled(),
                streams = new
                {
                    success_count = Interlocked.Read(ref _streamSuccessCount),
                    failure_count = Interlocked.Read(ref _streamFailureCount),
                    total_duration_ms = Interlocked.Read(ref _totalStreamDurationMs),
                    total_output_chars = Interlocked.Read(ref _totalStreamOutputChars)
                },
                statuses = _statusCounters.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value),
                tools = _toolCounters.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value),
                errors = _errorCounters.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value)
            };
        }

        private bool IsEnabled()
        {
            return bool.TryParse(_configuration["AI:Telemetry:Enabled"], out var enabled) ? enabled : true;
        }
    }
}

