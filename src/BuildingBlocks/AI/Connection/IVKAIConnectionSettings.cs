namespace VK.Blocks.AI;

/// <summary>
/// Defines connection parameters for AI features.
/// </summary>
public interface IVKAIConnectionSettings
{
    /// <summary>
    /// Gets the specific provider for the feature.
    /// If null, falls back to global AI options.
    /// </summary>
    VKAIProviderType? Provider { get; init; }

    /// <summary>
    /// Gets the specific model identifier for the feature.
    /// </summary>
    string? ModelId { get; init; }
}
