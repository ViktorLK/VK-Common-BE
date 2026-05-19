using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Aggregates all Orchestration configuration options.
/// </summary>
public interface IVKOrchestrationOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the default intent if none is identified.
    /// </summary>
    VKIntent? DefaultIntent { get; }

    /// <summary>
    /// Gets the confidence threshold for intent identification.
    /// </summary>
    double? ConfidenceThreshold { get; }
}
