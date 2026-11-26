using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VK.Blocks.AI.Synapse.Quota.Internal;

/// <summary>
/// Bounded thread-safe metrics collector tracking exponential moving average latency and recent token usage per connection.
/// </summary>
// [AP.01] [CS.06]
internal sealed class DefaultAIMetricsCollector : IVKAIMetricsCollector
{
    private const int MaxTrackedKeys = 4096;

    private sealed class MetricsState
    {
        public List<(DateTimeOffset Time, int Count)> TokenUsages { get; } = new();
        public double AverageLatencyMs { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; }
        public object LockObject { get; } = new();
    }

    private readonly ConcurrentDictionary<string, MetricsState> _states = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeProvider _timeProvider;

    public DefaultAIMetricsCollector(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public void RecordMetrics(VKAIConnection connection, int tokens, TimeSpan latency)
    {
        if (connection is null || string.IsNullOrWhiteSpace(connection.Id))
        {
            return;
        }

        var now = _timeProvider.GetUtcNow();
        var state = GetOrCreateState(connection, now);

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

            state.LastUpdatedAt = now;
            state.TokenUsages.Add((now, tokens));
            PruneOldTokens(state, now);
        }
    }

    public double GetAverageLatencyMs(VKAIConnection connection)
    {
        if (connection is null || string.IsNullOrWhiteSpace(connection.Id))
        {
            return 0;
        }

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

    private MetricsState GetOrCreateState(VKAIConnection connection, DateTimeOffset now)
    {
        var key = GetConnectionKey(connection);
        if (_states.TryGetValue(key, out var existing))
        {
            return existing;
        }

        if (_states.Count >= MaxTrackedKeys)
        {
            EvictStaleOrOldest(now);
        }

        return _states.GetOrAdd(key, _ => new MetricsState { LastUpdatedAt = now });
    }

    private void EvictStaleOrOldest(DateTimeOffset now)
    {
        var cutoff = now.AddMinutes(-5);
        string? oldestKey = null;
        DateTimeOffset oldestTime = DateTimeOffset.MaxValue;

        foreach (var pair in _states)
        {
            DateTimeOffset updatedAt;
            lock (pair.Value.LockObject)
            {
                updatedAt = pair.Value.LastUpdatedAt;
            }

            if (updatedAt < cutoff && _states.TryRemove(KeyValuePair.Create(pair.Key, pair.Value)))
            {
                if (_states.Count < MaxTrackedKeys)
                {
                    return;
                }

                continue;
            }

            if (updatedAt < oldestTime)
            {
                oldestTime = updatedAt;
                oldestKey = pair.Key;
            }
        }

        if (_states.Count >= MaxTrackedKeys && oldestKey is not null)
        {
            _states.TryRemove(oldestKey, out _);
        }
    }

    private static void PruneOldTokens(MetricsState state, DateTimeOffset now)
    {
        var cutoff = now.AddMinutes(-5);
        state.TokenUsages.RemoveAll(x => x.Time < cutoff);
    }

    private static string GetConnectionKey(VKAIConnection connection)
    {
        return connection.Id;
    }
}
