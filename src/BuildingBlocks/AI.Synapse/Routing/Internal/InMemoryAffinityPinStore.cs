using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Synapse.Routing.Internal;

/// <summary>
/// Bounded thread-safe in-memory fallback implementation of <see cref="IVKAIAffinityPinStore"/> with TTL expiration.
/// </summary>
// [AP.01] [ARCH-05] [CS.06]
internal sealed class InMemoryAffinityPinStore : IVKAIAffinityPinStore
{
    private const int MaxTrackedKeys = 4096;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(30);

    private readonly ConcurrentDictionary<string, (string ConnectionId, DateTimeOffset ExpiresAt)> _pins = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeProvider _timeProvider;

    public InMemoryAffinityPinStore(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public ValueTask<string?> GetPinnedConnectionAsync(string affinityKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(affinityKey))
        {
            return ValueTask.FromResult<string?>(null);
        }

        if (_pins.TryGetValue(affinityKey, out var entry))
        {
            if (entry.ExpiresAt > _timeProvider.GetUtcNow())
            {
                return ValueTask.FromResult<string?>(entry.ConnectionId);
            }

            _pins.TryRemove(KeyValuePair.Create(affinityKey, entry));
        }

        return ValueTask.FromResult<string?>(null);
    }

    public ValueTask PinIfUnsetAsync(string affinityKey, string connectionId, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(affinityKey) || string.IsNullOrWhiteSpace(connectionId))
        {
            return ValueTask.CompletedTask;
        }

        var now = _timeProvider.GetUtcNow();
        var expiresAt = now.Add(ttl ?? DefaultTtl);

        if (!_pins.ContainsKey(affinityKey) && _pins.Count >= MaxTrackedKeys)
        {
            EvictExpiredOrOldest(now);
        }

        _pins.AddOrUpdate(
            affinityKey,
            _ => (connectionId, expiresAt),
            (_, existing) => existing.ExpiresAt <= now ? (connectionId, expiresAt) : existing);

        return ValueTask.CompletedTask;
    }

    public ValueTask UnpinAsync(string affinityKey, string expectedConnectionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(affinityKey) || string.IsNullOrWhiteSpace(expectedConnectionId))
        {
            return ValueTask.CompletedTask;
        }

        if (_pins.TryGetValue(affinityKey, out var entry) &&
            string.Equals(entry.ConnectionId, expectedConnectionId, StringComparison.OrdinalIgnoreCase))
        {
            _pins.TryRemove(KeyValuePair.Create(affinityKey, entry));
        }

        return ValueTask.CompletedTask;
    }

    private void EvictExpiredOrOldest(DateTimeOffset now)
    {
        string? earliestKey = null;
        DateTimeOffset earliestExpiry = DateTimeOffset.MaxValue;

        foreach (var pair in _pins)
        {
            if (pair.Value.ExpiresAt <= now)
            {
                _pins.TryRemove(pair);
                if (_pins.Count < MaxTrackedKeys)
                {
                    return;
                }

                continue;
            }

            if (pair.Value.ExpiresAt < earliestExpiry)
            {
                earliestExpiry = pair.Value.ExpiresAt;
                earliestKey = pair.Key;
            }
        }

        if (_pins.Count >= MaxTrackedKeys && earliestKey is not null)
        {
            _pins.TryRemove(earliestKey, out _);
        }
    }
}
