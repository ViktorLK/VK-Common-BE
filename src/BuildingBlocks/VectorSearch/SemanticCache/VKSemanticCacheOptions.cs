using System;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for the Semantic Cache.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock))]
public sealed partial record VKSemanticCacheOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Semantic Cache is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the similarity score threshold above which a cache hit is registered.
    /// </summary>
    public double ScoreThreshold { get; init; } = 0.95;

    /// <summary>
    /// Gets or sets the collection name used to store cached vectors.
    /// </summary>
    public string CollectionName { get; init; } = "SemanticCache";

    /// <summary>
    /// Gets or sets the Time to Live (TTL) for cached items.
    /// </summary>
    public TimeSpan Ttl { get; init; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets a value indicating whether sliding expiration is enabled.
    /// If true, a cache hit extends the item's lifetime.
    /// </summary>
    public bool SlidingExpiration { get; init; } = false;
}
