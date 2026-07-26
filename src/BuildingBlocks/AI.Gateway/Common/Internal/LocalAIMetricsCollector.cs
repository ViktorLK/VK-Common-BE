using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VK.Blocks.AI.Gateway.Internal;

internal sealed class LocalAIMetricsCollector : IAIMetricsCollector
{
    private sealed class MetricsState
    {
        public List<(DateTimeOffset Time, int Count)> TokenUsages { get; } = new();
        public double AverageLatencyMs { get; set; }
        public object LockObject { get; } = new();
    }

    private readonly ConcurrentDictionary<string, MetricsState> _states = new();
    private readonly TimeProvider _timeProvider;

    public LocalAIMetricsCollector(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public void RecordMetrics(IVKAIProviderOptions config, int tokens, TimeSpan latency)
    {
        if (config == null)
            return;
        var state = GetOrCreateState(config);
        var now = _timeProvider.GetUtcNow();

        lock (state.LockObject)
        {
            if (state.AverageLatencyMs == 0)
            {
                state.AverageLatencyMs = latency.TotalMilliseconds;
            }
            else
            {
                state.AverageLatencyMs = (state.AverageLatencyMs * 0.8) + (latency.TotalMilliseconds * 0.2);
            }

            if (tokens > 0)
            {
                state.TokenUsages.Add((now, tokens));
            }

            CleanOldTokenUsages(state, now);
        }
    }

    public double GetAverageLatencyMs(IVKAIProviderOptions config)
    {
        if (config == null)
            return 0;
        var state = GetOrCreateState(config);
        lock (state.LockObject)
        {
            return state.AverageLatencyMs;
        }
    }

    private MetricsState GetOrCreateState(IVKAIProviderOptions config)
    {
        var key = GetProviderKey(config);
        return _states.GetOrAdd(key, _ => new MetricsState());
    }

    private string GetProviderKey(IVKAIProviderOptions options)
    {
        var keyStr = options.ApiKey?.ToString() ?? string.Empty;
        return $"{options.Provider}_{options.Endpoint}_{options.ModelId}_{keyStr.GetHashCode()}";
    }

    private void CleanOldTokenUsages(MetricsState state, DateTimeOffset now)
    {
        var oneMinuteAgo = now.AddMinutes(-1);
        state.TokenUsages.RemoveAll(tu => tu.Time < oneMinuteAgo);
    }
}
