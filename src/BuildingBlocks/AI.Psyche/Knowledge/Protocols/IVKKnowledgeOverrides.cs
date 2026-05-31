namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines knowledge settings that can be overridden at the request level.
/// </summary>
public interface IVKKnowledgeOverrides
{
    /// <summary>
    /// Gets the maximum number of knowledge entries to inject into context.
    /// </summary>
    int? MaxEntriesToInject { get; init; }

    /// <summary>
    /// Gets the number of tokens reserved for knowledge entries.
    /// </summary>
    int? ReservedTokens { get; init; }

    /// <summary>
    /// Gets the semantic similarity threshold for knowledge entry retrieval.
    /// </summary>
    float? SemanticThreshold { get; init; }

    /// <summary>
    /// Gets the maximum global recursion depth for key jumps and multi-hop retrieval.
    /// </summary>
    int? MaxGlobalRecursionDepth { get; init; }
}
