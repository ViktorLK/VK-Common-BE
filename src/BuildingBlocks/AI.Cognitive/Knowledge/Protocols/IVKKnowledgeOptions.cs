using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Aggregates all static Knowledge configuration options.
/// </summary>
public interface IVKKnowledgeOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the maximum number of knowledge entries to inject into context.
    /// </summary>
    int? MaxEntriesToInject { get; }

    /// <summary>
    /// Gets the number of tokens reserved for knowledge entries.
    /// </summary>
    int? ReservedTokens { get; }

    /// <summary>
    /// Gets the semantic similarity threshold for knowledge entry retrieval.
    /// </summary>
    float? SemanticThreshold { get; }

    /// <summary>
    /// Gets the maximum global recursion depth for key matching jumps and multi-hop retrieval.
    /// </summary>
    int? MaxGlobalRecursionDepth { get; }
}
