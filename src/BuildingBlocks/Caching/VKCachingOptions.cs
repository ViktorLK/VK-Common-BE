using System;
using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Default configuration settings for the Caching building block.
/// Follows AP.04, BB.05, BB.07.
/// </summary>
[VKOptions]
public sealed partial record VKCachingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the default expiration time for cache entries.
    /// </summary>
    public TimeSpan DefaultExpiration { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets the default local L1 expiration time for cache entries.
    /// </summary>
    public TimeSpan DefaultLocalExpiration { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets the prefix for all cache keys.
    /// </summary>
    public string KeyPrefix { get; init; } = string.Empty;

    /// <summary>
    /// Gets the maximum payload size in bytes for cache entries.
    /// </summary>
    public long? MaximumPayloadBytes { get; init; } = 10 * 1024 * 1024; // 10MB
}
