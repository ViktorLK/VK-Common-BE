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
    /// Gets the maximum global recursion depth for key matching jumps and multi-hop retrieval.
    /// </summary>
    int? MaxGlobalRecursionDepth { get; }

    /// <summary>
    /// Gets the semantic similarity threshold for knowledge entry retrieval.
    /// </summary>
    float? SemanticThreshold { get; }

    /// <summary>
    /// Gets the number of historical chat messages to look back for generating the semantic search context.
    /// Under AP.03, uses a Default Interface Property to maintain backward compatibility.
    /// </summary>
    int? SemanticScanDepth { get; }

    /// <summary>
    /// Gets the maximum number of semantic knowledge entries to retrieve from the vector store in a single query.
    /// Under AP.03, uses a Default Interface Property to maintain backward compatibility.
    /// </summary>
    int? SemanticMaxEntries { get; }
}
