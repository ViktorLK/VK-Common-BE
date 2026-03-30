namespace VK.Blocks.Caching.Options;

/// <summary>
/// Configuration for cache resilience strategies.
/// </summary>
public sealed class ResilienceOptions
{
    /// <summary>
    /// Enables protection against cache penetration (caching nulls).
    /// </summary>
    public bool EnablePenetrationProtection { get; set; } = true;

    /// <summary>
    /// TTL for the "Null" marker used in penetration protection.
    /// </summary>
    public TimeSpan NullCacheExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Enables protection against cache breakdown (hot key concurrency lock).
    /// </summary>
    public bool EnableBreakdownProtection { get; set; } = true;

    /// <summary>
    /// Enables protection against cache avalanche (jittered expiration).
    /// </summary>
    public bool EnableAvalancheProtection { get; set; } = true;

    /// <summary>
    /// Maximum jitter (randomness) to add to expiration times (0.0 to 1.0).
    /// </summary>
    public double MaxJitterRatio { get; set; } = 0.2;
}
