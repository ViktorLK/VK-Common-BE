namespace VK.Blocks.Caching.Resilience.Avalanche;

/// <summary>
/// Strategy for guarding against cache avalanche by jittering expiration times.
/// </summary>
public sealed class JitterExpiryStrategy : IJitterExpiryStrategy
{
    /// <inheritdoc />
    public TimeSpan ApplyJitter(TimeSpan expiration, double maxJitterRatio)
    {
        if (maxJitterRatio <= 0) return expiration;

        var jitterMs = expiration.TotalMilliseconds * maxJitterRatio * Random.Shared.NextDouble();
        return TimeSpan.FromMilliseconds(expiration.TotalMilliseconds + jitterMs);
    }
}
