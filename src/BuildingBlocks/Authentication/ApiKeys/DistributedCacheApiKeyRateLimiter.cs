using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace VK.Blocks.Authentication.ApiKeys;

/// <summary>
/// A rate limiter that prefers Redis atomic operations for performance and consistency,
/// but falls back to <see cref="IDistributedCache"/> if Redis is not available.
/// </summary>
public sealed class DistributedCacheApiKeyRateLimiter(
    IDistributedCache cache,
    ILogger<DistributedCacheApiKeyRateLimiter> logger,
    IConnectionMultiplexer? redis = null) : IApiKeyRateLimiter
{
    private readonly IDatabase? _db = redis?.GetDatabase();

    /// <inheritdoc />
    public async Task<bool> IsAllowedAsync(Guid keyId, int limitPerMinute, CancellationToken ct = default)
    {
        if (limitPerMinute <= 0)
        {
            return false;
        }

        if (_db is not null)
        {
            return await IsAllowedAtomicAsync(keyId, limitPerMinute);
        }

        // Fallback to non-atomic IDistributedCache implementation
        logger.LogWarning("Redis is not available. Using non-atomic IDistributedCache for rate limiting. Race conditions may occur.");
        return await IsAllowedFallbackAsync(keyId, limitPerMinute, ct);
    }

    private async Task<bool> IsAllowedAtomicAsync(Guid keyId, int limitPerMinute)
    {
        var windowKey = GetWindowKey(keyId);
        var count = await _db!.StringIncrementAsync(windowKey);

        if (count == 1)
        {
            await _db.KeyExpireAsync(windowKey, TimeSpan.FromSeconds(60));
        }

        return count <= limitPerMinute;
    }

    private async Task<bool> IsAllowedFallbackAsync(Guid keyId, int limitPerMinute, CancellationToken ct)
    {
        var windowKey = GetWindowKey(keyId);
        var countStr = await cache.GetStringAsync(windowKey, ct).ConfigureAwait(false);
        var count = countStr != null && int.TryParse(countStr, out var c) ? c : 0;

        if (count >= limitPerMinute)
        {
            return false;
        }

        count++;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        };

        await cache.SetStringAsync(windowKey, count.ToString(), options, ct).ConfigureAwait(false);
        return true;
    }

    private static string GetWindowKey(Guid keyId)
    {
        var now = DateTimeOffset.UtcNow;
        var minuteTimestamp = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        return $"ratelimit:apikey:{keyId}:{minuteTimestamp}";
    }
}

