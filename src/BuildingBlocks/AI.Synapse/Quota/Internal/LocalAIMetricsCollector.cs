using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VK.Blocks.AI.Synapse.Internal;

// [AP.01] sealed
internal sealed class LocalAIMetricsCollector : IVKAIMetricsCollector
{
    private sealed class MetricsState
    {
        public List<(DateTimeOffset Time, int Count)> TokenUsages { get; } = new();
        public double AverageLatencyMs { get; set; }
        public object LockObject { get; } = new();
    }

    private readonly ConcurrentDictionary<string, MetricsState> _states = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeProvider _timeProvider;

    public LocalAIMetricsCollector(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public void RecordMetrics(VKAIConnection connection, int tokens, TimeSpan latency)
    {
        if (connection is null)
            return;

        var state = GetOrCreateState(connection);
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

            state.TokenUsages.Add((now, tokens));
            PruneOldTokens(state, now);
        }
    }

    public double GetAverageLatencyMs(VKAIConnection connection)
    {
        if (connection is null)
            return 0;

        var key = GetConnectionKey(connection);
        if (_states.TryGetValue(key, out var state))
        {
            lock (state.LockObject)
            {
                return state.AverageLatencyMs;
            }
        }
        return 0;
    }

    private MetricsState GetOrCreateState(VKAIConnection connection)
    {
        var key = GetConnectionKey(connection);
        return _states.GetOrAdd(key, _ => new MetricsState());
    }

    private static void PruneOldTokens(MetricsState state, DateTimeOffset now)
    {
        var cutoff = now.AddMinutes(-5);
        state.TokenUsages.RemoveAll(x => x.Time < cutoff);
    }

    private static string GetConnectionKey(VKAIConnection connection)
    {
        return $"{connection.TenantId}_{connection.Id}";
    }
}
