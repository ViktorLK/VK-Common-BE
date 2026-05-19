using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Configuration settings for the Knowledge feature.
/// </summary>
[VKFeature(typeof(VKAICognitiveBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKKnowledgeOptions : IVKKnowledgeOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Knowledge feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum number of knowledge entries to inject into the prompt context.
    /// </summary>
    public int? MaxEntriesToInject { get; init; } = 5;

    /// <summary>
    /// Gets or sets the number of tokens reserved for knowledge entries in the prompt context.
    /// </summary>
    public int? ReservedTokens { get; init; } = 256;

    /// <summary>
    /// Gets or sets the maximum global recursion depth for key matching jumps and multi-hop retrieval.
    /// Defaults to 2.
    /// </summary>
    public int? MaxGlobalRecursionDepth { get; init; } = 2;

    /// <summary>
    /// Gets or sets the semantic similarity threshold for knowledge entry retrieval.
    /// </summary>
    public float? SemanticThreshold { get; init; } = 0.75f;

    /// <summary>
    /// Gets or sets the number of historical chat messages to look back for generating the semantic search context.
    /// Defaults to 5.
    /// </summary>
    public int? SemanticScanDepth { get; init; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of semantic knowledge entries to retrieve from the vector store in a single query.
    /// Defaults to 5.
    /// </summary>
    public int? SemanticMaxEntries { get; init; } = 5;
}
