namespace VK.Blocks.VectorSearch;

/// <summary>
/// Value Object: Represents a lightweight retrieved context slice ready for LLM Prompt injection.
/// Lives in VectorSearch (AI.Recall) boundary.
/// </summary>
public sealed record VKMemoryFragment
{
    /// <summary>
    /// Gets the unique identifier pointing back to the original memory trace or document chunk.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// Gets the exact textual content slice to be injected into the LLM context window.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the mathematical relevance score (e.g., Cosine Similarity / RRF Score) indicating how closely this fragment matched the query.
    /// </summary>
    public required float RelevanceScore { get; init; }

    /// <summary>
    /// Gets extended structural metadata associated with this fragment.
    /// </summary>
    public System.Collections.Generic.IReadOnlyDictionary<string, string> Metadata { get; init; } = new System.Collections.Generic.Dictionary<string, string>();
}
