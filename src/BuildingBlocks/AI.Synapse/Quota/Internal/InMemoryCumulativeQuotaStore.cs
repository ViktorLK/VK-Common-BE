using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;

namespace VK.Blocks.AI.Synapse.Quota.Internal;

/// <summary>
/// Bounded thread-safe in-memory fallback implementation of <see cref="IVKAICumulativeQuotaStore"/>.
/// </summary>
// [AP.01] [ARCH-05]
internal sealed class InMemoryCumulativeQuotaStore : IVKAICumulativeQuotaStore
{
    private const int MaxEntries = 10_000;

    private readonly ConcurrentDictionary<string, UsageAccumulator> _usages = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<string> _insertionOrder = new();

    public ValueTask<VKCumulativeQuotaUsage> GetUsageAsync(VKAIScopeKey scopeKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(scopeKey);
        if (_usages.TryGetValue(key, out var accumulator))
        {
            return ValueTask.FromResult(accumulator.Snapshot());
        }

        return ValueTask.FromResult(new VKCumulativeQuotaUsage());
    }

    public ValueTask RecordUsageAsync(VKAIScopeKey scopeKey, long tokens, decimal cost, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (tokens <= 0 && cost <= 0m)
        {
            return ValueTask.CompletedTask;
        }

        var key = BuildKey(scopeKey);
        var accumulator = _usages.GetOrAdd(key, k =>
        {
            _insertionOrder.Enqueue(k);
            EvictIfNeeded();
            return new UsageAccumulator();
        });

        accumulator.Add(tokens, cost);
        return ValueTask.CompletedTask;
    }

    private void EvictIfNeeded()
    {
        while (_usages.Count > MaxEntries && _insertionOrder.TryDequeue(out var oldestKey))
        {
            _usages.TryRemove(oldestKey, out _);
        }
    }

    private static string BuildKey(VKAIScopeKey scopeKey)
    {
        return scopeKey.Value ?? string.Empty;
    }

    private sealed class UsageAccumulator
    {
        private readonly object _gate = new();
        private long _totalTokens;
        private decimal _totalCost;

        public void Add(long tokens, decimal cost)
        {
            lock (_gate)
            {
                if (tokens > 0)
                {
                    _totalTokens += tokens;
                }

                if (cost > 0m)
                {
                    _totalCost += cost;
                }
            }
        }

        public VKCumulativeQuotaUsage Snapshot()
        {
            lock (_gate)
            {
                return new VKCumulativeQuotaUsage
                {
                    TotalTokens = _totalTokens,
                    TotalCost = _totalCost
                };
            }
        }
    }
}
