using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Options contract for the Presence feature.
/// </summary>
public interface IVKPresenceOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the sentiment sensitivity threshold.
    /// </summary>
    float SentimentThreshold { get; }

    /// <summary>
    /// Gets the default environmental scenario context.
    /// </summary>
    string? Scenario { get; }
}
