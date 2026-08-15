using System;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Synapse.Internal;

// [AP.01] sealed
internal sealed class LocalAIRateLimiter : IVKAIRateLimiter
{
    private readonly IVKBulkhead _bulkhead;
    private readonly IVKRateLimiter _rateLimiter;
    private readonly VKQuotaOptions _defaults;

    public LocalAIRateLimiter(
        IVKBulkhead bulkhead,
        IVKRateLimiter rateLimiter,
        VKQuotaOptions defaults)
    {
        _bulkhead = VKGuard.NotNull(bulkhead);
        _rateLimiter = VKGuard.NotNull(rateLimiter);
        _defaults = VKGuard.NotNull(defaults);
    }

    public bool IsAllowed(VKAIConnection connection)
    {
        if (connection is null)
            return false;

        var key = GetConnectionKey(connection);

        // 1. Check Bulkhead (In-Flight concurrency limit from connection or default)
        int maxConcurrency = connection.MaxConcurrency > 0 ? connection.MaxConcurrency : _defaults.DefaultMaxConcurrency;
        if (!_bulkhead.IsAllowed(key, maxConcurrency))
        {
            return false;
        }

        // 2. Check RateLimiter (RPM throughput limit)
        if (_defaults.DefaultRequestsPerMinute > 0)
        {
            if (!_rateLimiter.IsAllowed(key, _defaults.DefaultRequestsPerMinute, TimeSpan.FromMinutes(1)))
            {
                return false;
            }
        }

        return true;
    }

    public void Acquire(VKAIConnection connection)
    {
        if (connection is null)
            return;

        var key = GetConnectionKey(connection);
        _bulkhead.Acquire(key);
        _rateLimiter.RecordRequest(key);
    }

    public void Release(VKAIConnection connection)
    {
        if (connection is null)
            return;

        var key = GetConnectionKey(connection);
        _bulkhead.Release(key);
    }

    private static string GetConnectionKey(VKAIConnection connection)
    {
        return $"{connection.TenantId}_{connection.Id}";
    }
}
