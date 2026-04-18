using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Authentication.Common;
using VK.Blocks.Authentication.Features.ApiKeys.Persistence;

namespace VK.Blocks.Authentication.Features.ApiKeys.Internal;

/// <summary>
/// A high-performance, zero-dependency InMemory rate limiter using <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// Suitable for single-instance deployments.
/// </summary>
public sealed class InMemoryApiKeyRateLimiter(TimeProvider timeProvider) : IApiKeyRateLimiter, IInMemoryCacheCleanup, IAsyncDisposable
{
    private readonly ConcurrentDictionary<Guid, RateLimitState> _cache = new();
    private readonly object _cleanupLock = new();

    /// <inheritdoc />
    public Type AssociatedServiceType => typeof(IApiKeyRateLimiter);

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _cache.Clear();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<bool> IsAllowedAsync(Guid keyId, int limit, int windowSeconds, CancellationToken ct = default)
    {
        if (limit <= 0)
        {
            return ValueTask.FromResult(false);
        }

        var now = timeProvider.GetUtcNow().ToUnixTimeSeconds();
        var state = _cache.GetOrAdd(keyId, _ => new RateLimitState());

        lock (state)
        {
            state.LastTouchedAt = now;
            var windowStart = now - windowSeconds;

            // 1. Remove expired timestamps (Slide the window)
            while (state.Timestamps.TryPeek(out var timestamp) && timestamp < windowStart)
            {
                state.Timestamps.TryDequeue(out _);
            }

            // 2. Check limit
            if (state.Timestamps.Count < limit)
            {
                state.Timestamps.Enqueue(now);
                return ValueTask.FromResult(true);
            }

            return ValueTask.FromResult(false);
        }
    }

    /// <inheritdoc />
    public void CleanupExpiredEntries()
    {
        if (!Monitor.TryEnter(_cleanupLock))
        {
            return;
        }

        try
        {
            var now = timeProvider.GetUtcNow().ToUnixTimeSeconds();
            // Remove keys that haven't been seen for more than an hour to prevent memory leaks
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.LastTouchedAt < now - 3600)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }
        }
        finally
        {
            Monitor.Exit(_cleanupLock);
        }
    }

    private sealed class RateLimitState
    {
        public ConcurrentQueue<long> Timestamps { get; } = new();
        public long LastTouchedAt { get; set; }
    }
}
