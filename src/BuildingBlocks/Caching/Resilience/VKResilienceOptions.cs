using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Options for the Caching Resilience feature slice.
/// </summary>

public sealed partial record VKResilienceOptions : IVKBlockOptions
{
    /// <summary>
    /// Enables protection against cache penetration (caching nulls).
    /// </summary>
    public bool EnablePenetrationProtection { get; init; } = true;

    /// <summary>
    /// TTL for the "Null" marker used in penetration protection.
    /// </summary>
    public TimeSpan NullCacheExpiration { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Enables protection against cache breakdown (hot key concurrency lock).
    /// </summary>
    public bool EnableBreakdownProtection { get; init; } = true;

    /// <summary>
    /// Enables protection against cache avalanche (jittered expiration).
    /// </summary>
    public bool EnableAvalancheProtection { get; init; } = true;

    /// <summary>
    /// Maximum jitter (randomness) to add to expiration times (0.0 to 1.0).
    /// </summary>
    public double MaxJitterRatio { get; init; } = 0.2;
}
