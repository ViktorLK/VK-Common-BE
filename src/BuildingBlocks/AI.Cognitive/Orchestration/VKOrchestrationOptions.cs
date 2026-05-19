using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Options for AI orchestration.
/// </summary>
[VKFeature(typeof(VKAICognitiveBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKOrchestrationOptions : IVKOrchestrationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether orchestration is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default intent if none is identified.
    /// </summary>
    public VKIntent? DefaultIntent { get; init; } = VKIntent.Chat;

    /// <summary>
    /// Gets or sets the confidence threshold for intent identification.
    /// </summary>
    public double? ConfidenceThreshold { get; init; } = 0.5;
}
