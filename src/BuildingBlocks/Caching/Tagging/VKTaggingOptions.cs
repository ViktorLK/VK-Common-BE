using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Options for the Caching Tagging feature slice.
/// </summary>

public sealed partial record VKTaggingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the channel name for invalidation events propagation.
    /// </summary>
    public string InvalidationChannel { get; init; } = "cache:invalidation";
}
