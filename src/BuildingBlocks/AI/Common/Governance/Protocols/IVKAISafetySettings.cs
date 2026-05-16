namespace VK.Blocks.AI;

/// <summary>
/// Defines content safety parameters for AI features.
/// </summary>
public interface IVKAISafetySettings
{
    /// <summary>
    /// Gets a value indicating whether to enable content safety filtering for this specific feature.
    /// </summary>
    bool? EnableContentFilter { get; init; }
}
