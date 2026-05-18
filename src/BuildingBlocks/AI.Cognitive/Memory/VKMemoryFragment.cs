namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Value Object: Represents a lightweight context slice retrieved for LLM Prompt injection.
/// <para>
/// When the Echoes (Vector DB) system is queried, it does not return the heavy, biologically active 
/// <see cref="VKMemoryTrace"/>. Instead, it returns a Fragment—a pure, read-only slice of context 
/// accompanied by a Relevance Score, ready to be woven into the Prompt Tapestry (RAG).
/// </para>
/// </summary>
public sealed record VKMemoryFragment
{
    /// <summary>
    /// Gets the unique identifier pointing back to the original <see cref="VKMemoryTrace"/> or <see cref="VKMemorySynopsis"/>.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// Gets the exact textual content slice to be injected into the LLM context window.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the mathematical relevance score (e.g., Cosine Similarity) indicating how closely this fragment matched the query.
    /// </summary>
    public required float RelevanceScore { get; init; }

    /// <summary>
    /// Gets the cognitive category of the original memory (e.g., used for filtering contextual injection).
    /// </summary>
    public VKMemoryCategory Category { get; init; } = VKMemoryCategory.ShortTerm;
}
