namespace VK.Blocks.AI;

/// <summary>
/// Defines safety parameters that can be overridden at the request level.
/// </summary>
public interface IVKAISafetyOverrides
{
    /// <summary>
    /// Gets a value indicating whether to enable content safety filtering for this specific request.
    /// </summary>
    bool? EnableContentFilter { get; init; }
}
