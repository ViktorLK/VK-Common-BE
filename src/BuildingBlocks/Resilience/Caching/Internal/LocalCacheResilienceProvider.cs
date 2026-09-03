using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Diagnostics.Internal;

namespace VK.Blocks.Resilience.Caching.Internal;

// [AP.01] sealed
internal sealed class LocalCacheResilienceProvider : IVKCacheResilienceProvider
{
    private sealed class CacheEntry
    {
        public object? Value { get; set; }
        public DateTimeOffset FreshUntil { get; set; }
        public DateTimeOffset StaleUntil { get; set; }
        public bool IsRefreshing { get; set; }
        public object LockObject { get; } = new();
    }

    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly TimeProvider _timeProvider;

    public LocalCacheResilienceProvider(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<VKResult<T>> ExecuteWithStaleWhileRevalidateAsync<T>(
        string cacheKey,
        Func<CancellationToken, Task<VKResult<T>>> fetchSource,
        TimeSpan freshDuration,
        TimeSpan staleDuration,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(cacheKey);
        VKGuard.NotNull(fetchSource);
        cancellationToken.ThrowIfCancellationRequested();

        var now = _timeProvider.GetUtcNow();
        var entry = _cache.GetOrAdd(cacheKey, _ => new CacheEntry());

        bool shouldTriggerBackgroundRefresh = false;
        T? cachedValue = default;
        bool hasUsableCache = false;

        lock (entry.LockObject)
        {
            if (entry.Value is T val)
            {
                if (now <= entry.FreshUntil)
                {
                    // Cache is completely fresh
                    ResilienceDiagnostics.RecordStrategyExecution("swr_fresh_hit", true);
                    return VKResult.Success(val);
                }

                if (now <= entry.StaleUntil)
                {
                    // Cache is stale but acceptable
                    cachedValue = val;
                    hasUsableCache = true;

                    if (!entry.IsRefreshing)
                    {
                        entry.IsRefreshing = true;
                        shouldTriggerBackgroundRefresh = true;
                    }
                }
            }
        }

        if (shouldTriggerBackgroundRefresh)
        {
            // Fire-and-forget background refresh
            _ = Task.Run(async () =>
            {
                try
                {
                    var freshResult = await fetchSource(CancellationToken.None).ConfigureAwait(false);
                    if (freshResult.IsSuccess)
                    {
                        var refreshNow = _timeProvider.GetUtcNow();
                        lock (entry.LockObject)
                        {
                            entry.Value = freshResult.Value;
                            entry.FreshUntil = refreshNow.Add(freshDuration);
                            entry.StaleUntil = refreshNow.Add(freshDuration + staleDuration);
                        }
                    }
                }
                catch
                {
                    // Background refresh failure suppressed
                }
                finally
                {
                    lock (entry.LockObject)
                    {
                        entry.IsRefreshing = false;
                    }
                }
            }, CancellationToken.None);
        }

        if (hasUsableCache && cachedValue is not null)
        {
            ResilienceDiagnostics.RecordStrategyExecution("swr_stale_hit", true);
            return VKResult.Success(cachedValue);
        }

        // Cache miss or hard-expired -> synchronous fetch
        var sourceResult = await fetchSource(cancellationToken).ConfigureAwait(false);
        if (sourceResult.IsSuccess)
        {
            var updateNow = _timeProvider.GetUtcNow();
            lock (entry.LockObject)
            {
                entry.Value = sourceResult.Value;
                entry.FreshUntil = updateNow.Add(freshDuration);
                entry.StaleUntil = updateNow.Add(freshDuration + staleDuration);
            }
            ResilienceDiagnostics.RecordStrategyExecution("swr_source_fetch", true);
        }
        else
        {
            ResilienceDiagnostics.RecordStrategyExecution("swr_source_fetch", false);
        }

        return sourceResult;
    }
}
