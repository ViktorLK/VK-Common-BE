using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.RateLimiting.Internal;

// [AP.01] sealed
internal sealed class LocalRateLimiter : IVKRateLimiter
{
    private sealed class LimiterBucket
    {
        public List<DateTimeOffset> RequestTimestamps { get; } = new();
        public object LockObject { get; } = new();
    }

    private readonly ConcurrentDictionary<string, LimiterBucket> _buckets = new();
    private readonly TimeProvider _timeProvider;

    public LocalRateLimiter(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public bool IsAllowed(string key, int permitLimit, TimeSpan? window = null)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        if (permitLimit <= 0)
            return true;

        var bucket = _buckets.GetOrAdd(key, _ => new LimiterBucket());
        var now = _timeProvider.GetUtcNow();
        var effectiveWindow = window ?? TimeSpan.FromMinutes(1);
        var windowStart = now.Subtract(effectiveWindow);

        lock (bucket.LockObject)
        {
            bucket.RequestTimestamps.RemoveAll(t => t < windowStart);
            return bucket.RequestTimestamps.Count < permitLimit;
        }
    }

    public void RecordRequest(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var bucket = _buckets.GetOrAdd(key, _ => new LimiterBucket());
        var now = _timeProvider.GetUtcNow();

        lock (bucket.LockObject)
        {
            bucket.RequestTimestamps.Add(now);
        }
    }
}
