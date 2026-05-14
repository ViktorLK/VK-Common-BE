namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Arguments for knowledge retrieval and injection.
/// </summary>
public sealed record VKKnowledgeArgs
{
    /// <summary>
    /// Gets the maximum number of knowledge entries to inject.
    /// </summary>
    public int? MaxEntriesToInject { get; init; }

    /// <summary>
    /// Gets the semantic threshold for knowledge retrieval.
    /// </summary>
    public float? SemanticThreshold { get; init; }
}
