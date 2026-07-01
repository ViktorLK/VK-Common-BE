namespace VK.Blocks.AI;

/// <summary>
/// Defines content safety parameters for AI features.
/// </summary>
public interface IVKAISafetyOptions
{
    /// <summary>
    /// Gets a value indicating whether to enable content safety filtering for this specific feature.
    /// </summary>
    bool? EnableContentFilter { get; init; }
}
