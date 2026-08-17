namespace VK.Blocks.Caching.Resilience.Avalanche;

/// <summary>
/// Strategy contract for applying jitter to cache expiration times.
/// </summary>
internal interface IJitterExpiryStrategy
{
    /// <summary>
    /// Applies jitter to the specified expiration time.
    /// </summary>
    TimeSpan ApplyJitter(TimeSpan expiration, double maxJitterRatio);
}
