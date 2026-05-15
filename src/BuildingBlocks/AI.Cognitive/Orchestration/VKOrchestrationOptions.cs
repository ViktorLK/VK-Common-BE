using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Options for AI orchestration.
/// </summary>
public sealed record VKOrchestrationOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for Orchestration options.
    /// </summary>
    public static string SectionName => "AI:Orchestration";

    /// <summary>
    /// Gets or sets a value indicating whether orchestration is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default intent if none is identified.
    /// </summary>
    public VKIntent DefaultIntent { get; init; } = VKIntent.Chat;

    /// <summary>
    /// Gets or sets the confidence threshold for intent identification.
    /// </summary>
    public double ConfidenceThreshold { get; init; } = 0.5;
}
