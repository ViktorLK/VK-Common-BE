namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines orchestration settings that can be overridden at the request level.
/// </summary>
public interface IVKOrchestrationOverrides
{
    /// <summary>
    /// Gets the default intent if none is identified.
    /// </summary>
    VKIntent? DefaultIntent { get; init; }

    /// <summary>
    /// Gets the confidence threshold for intent identification.
    /// </summary>
    double? ConfidenceThreshold { get; init; }
}
