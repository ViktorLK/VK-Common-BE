namespace VK.Blocks.Caching.Abstractions.Contracts;

/// <summary>
/// Configuration for a specific cache operation.
/// </summary>
public sealed record CacheOptions
{
    /// <summary>
    /// Gets or sets the absolute expiration relative to now.
    /// </summary>
    public TimeSpan? Expiration { get; init; }

    /// <summary>
    /// Gets or sets the sliding expiration.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; init; }

    /// <summary>
    /// Gets or sets whether to bypass the cache and fetch from the source.
    /// </summary>
    public bool BypassCache { get; init; }

    /// <summary>
    /// Gets or sets whether to suppress errors from the cache provider.
    /// </summary>
    public bool SuppressErrors { get; init; } = true;

    /// <summary>
    /// Default options.
    /// </summary>
    public static readonly CacheOptions Default = new();
}
