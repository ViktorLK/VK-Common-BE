using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Cost.Internal;

// [AP.01] [ARCH-05]
internal sealed class InMemoryAICostLedger : IVKAICostLedger
{
    private const int MaxEntries = 10_000;

    private readonly ConcurrentDictionary<string, VKAICostLedgerEntry> _entries = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<string> _insertionOrder = new();

    public ValueTask<bool> RecordAsync(VKAICostLedgerEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entry);
        VKGuard.NotNullOrWhiteSpace(entry.InvocationId);

        var key = $"{entry.InvocationId}:{entry.AttemptNo}";
        bool added = _entries.TryAdd(key, entry);
        if (added)
        {
            _insertionOrder.Enqueue(key);
            EvictIfNeeded();
        }

        return ValueTask.FromResult(added);
    }

    private void EvictIfNeeded()
    {
        while (_entries.Count > MaxEntries && _insertionOrder.TryDequeue(out string? oldestKey))
        {
            _entries.TryRemove(oldestKey, out _);
        }
    }

    public ValueTask<decimal> GetTotalCostAsync(
        VKAIScopeKey scopeKey,
        VKAICorrelationTags? tags = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        decimal total = _entries.Values
            .Where(e => Matches(e, scopeKey, tags) && e.Cost.IsPriced)
            .Sum(e => e.Cost.Amount);

        return ValueTask.FromResult(total);
    }

    public ValueTask<IReadOnlyList<VKAICostLedgerEntry>> GetEntriesAsync(
        VKAIScopeKey scopeKey,
        VKAICorrelationTags? tags = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<VKAICostLedgerEntry> results = _entries.Values
            .Where(e => Matches(e, scopeKey, tags))
            .ToArray();

        return ValueTask.FromResult(results);
    }

    private static bool Matches(VKAICostLedgerEntry entry, VKAIScopeKey scopeKey, VKAICorrelationTags? filterTags)
    {
        if (entry.ScopeKey != scopeKey)
        {
            return false;
        }

        if (filterTags is null)
        {
            return true;
        }

        if (!string.IsNullOrEmpty(filterTags.RunId) &&
            !string.Equals(entry.Tags?.RunId, filterTags.RunId, StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(filterTags.SessionId) &&
            !string.Equals(entry.Tags?.SessionId, filterTags.SessionId, StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }
}
