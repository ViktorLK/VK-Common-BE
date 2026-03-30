namespace VK.Blocks.Caching.Resilience.Avalanche;

/// <summary>
/// Strategy for applying jitter to cache expiration to prevent cache avalanche.
/// </summary>
public interface IJitterExpiryStrategy
{
    /// <summary>
    /// Applies a random jitter to the expiration time.
    /// </summary>
    /// <param name="expiration">Base expiration timespan.</param>
    /// <param name="maxJitterRatio">Maximum ratio of jitter (0.0 to 1.0).</param>
    /// <returns>Jittered expiration timespan.</returns>
    TimeSpan ApplyJitter(TimeSpan expiration, double maxJitterRatio);
}
