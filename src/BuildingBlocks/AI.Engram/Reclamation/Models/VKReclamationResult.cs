namespace VK.Blocks.AI.Engram;

/// <summary>
/// Execution summary result for a memory reclamation cycle.
/// </summary>
public sealed record VKReclamationResult
{
    /// <summary>
    /// Gets the number of memory entries evaluated for decay during the cycle.
    /// </summary>
    public int EvaluatedCount { get; init; }

    /// <summary>
    /// Gets the number of memory entries decayed during the cycle.
    /// </summary>
    public int DecayedCount { get; init; }

    /// <summary>
    /// Gets the number of memory entries pruned (deleted or archived) during the cycle.
    /// </summary>
    public int PrunedCount { get; init; }

    /// <summary>
    /// Gets the number of vector embeddings cascade-deleted from VectorStore.
    /// </summary>
    public int VectorStoreCleanedCount { get; init; }
}
