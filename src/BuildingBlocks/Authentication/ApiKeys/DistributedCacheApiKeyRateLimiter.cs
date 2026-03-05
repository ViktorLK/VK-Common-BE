using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace VK.Blocks.Authentication.ApiKeys;

/// <summary>
/// A simple fixed-window rate limiter utilizing <see cref="IDistributedCache"/>.
/// </summary>
public sealed class DistributedCacheApiKeyRateLimiter(IDistributedCache cache) : IApiKeyRateLimiter
{
    /// <inheritdoc />
    public async Task<bool> IsAllowedAsync(Guid keyId, int limitPerMinute, CancellationToken ct = default)
    {
        if (limitPerMinute <= 0)
        {
            return false; // Automatically block if limit is zero or negative
        }

        var windowKey = GetWindowKey(keyId);

        // Fetch the current request count for this minute window
        var countStr = await cache.GetStringAsync(windowKey, ct).ConfigureAwait(false);
        var count = countStr != null && int.TryParse(countStr, out var c) ? c : 0;

        if (count >= limitPerMinute)
        {
            return false;
        }

        // Increment the counter
        count++;

        var options = new DistributedCacheEntryOptions
        {
            // Expire at the end of the current minute
            AbsoluteExpiration = GetEndOfCurrentMinute()
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

    private static DateTimeOffset GetEndOfCurrentMinute()
    {
        var now = DateTimeOffset.UtcNow;
        return new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeSpan.Zero).AddMinutes(1);
    }
}
