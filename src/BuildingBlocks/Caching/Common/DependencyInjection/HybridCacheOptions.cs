namespace VK.Blocks.Caching;

/// <summary>
/// Configuration for Hybrid Caching (L1 Memory + L2 Redis).
/// </summary>
public sealed record HybridCacheOptions
{
    /// <summary>
    /// Default expiration for L1 (Memory) cache.
    /// Typically shorter than L2.
    /// </summary>
    public TimeSpan L1DefaultExpiration { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Default expiration for L2 (Redis) cache.
    /// </summary>
    public TimeSpan L2DefaultExpiration { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Whether to enable L1 cache.
    /// </summary>
    public bool EnableL1 { get; init; } = true;

    /// <summary>
    /// Whether to propagate deletions from L1 to L2 and vice versa.
    /// </summary>
    public bool EnablePropagation { get; init; } = true;
}
