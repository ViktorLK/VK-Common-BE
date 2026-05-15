using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Configuration settings for the Knowledge feature.
/// </summary>
public sealed record VKKnowledgeOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for Knowledge options.
    /// </summary>
    public static string SectionName => VKAICognitiveOptions.SectionName + ":" + VKAICognitiveOptions.KnowledgeSection;

    /// <summary>
    /// Gets or sets a value indicating whether Knowledge feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum number of knowledge entries to inject into the prompt context.
    /// </summary>
    public int MaxEntriesToInject { get; init; } = 5;

    /// <summary>
    /// Gets or sets the number of tokens reserved for knowledge entries in the prompt context.
    /// </summary>
    public int ReservedTokens { get; init; } = 256;

    /// <summary>
    /// Gets or sets the semantic similarity threshold for knowledge entry retrieval.
    /// </summary>
    public float SemanticThreshold { get; init; } = 0.75f;
}
