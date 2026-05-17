using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Default configuration settings for the AI Cognitive building block.
/// These values serve as fallbacks for all cognitive features.
/// </summary>
[VKFeature(typeof(VKAICognitiveBlock), GenerateValidator = true, Namespace = "VK.Blocks.AI.Cognitive.Common.DependencyInjection")]
public sealed partial record VKAICognitiveDefaultsOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the default persona ID to use across cognitive features.
    /// </summary>
    public string? DefaultPersonaId { get; init; } = "Default";

    /// <summary>
    /// Gets the default minimum score threshold for memory search.
    /// </summary>
    public float? DefaultMinScore { get; init; } = 0.7f;

    /// <summary>
    /// Gets the default confidence threshold for intent arbiter.
    /// </summary>
    public double? ConfidenceThreshold { get; init; } = 0.6;

    /// <summary>
    /// Gets a value indicating whether to allow parallel execution of reasoning steps by default.
    /// </summary>
    public bool? AllowParallelism { get; init; } = true;

    /// <summary>
    /// Gets the default maximum depth for reasoning steps.
    /// </summary>
    public int? MaxDepth { get; init; } = 3;
}
