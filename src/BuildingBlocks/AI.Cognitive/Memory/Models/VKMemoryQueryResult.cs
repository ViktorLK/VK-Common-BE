namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents the result of a memory query.
/// </summary>
public sealed record VKMemoryQueryResult
{
    /// <summary>
    /// Gets the memory entry.
    /// </summary>
    public required VKMemoryEntry Entry { get; init; }

    /// <summary>
    /// Gets the relevance score (0.0 to 1.0).
    /// </summary>
    public required float Score { get; init; }
}
