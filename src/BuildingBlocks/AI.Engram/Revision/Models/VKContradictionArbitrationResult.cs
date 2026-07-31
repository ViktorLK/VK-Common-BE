namespace VK.Blocks.AI.Engram;

/// <summary>
/// Details of a contradiction arbitration outcome between a new memory fact and existing memory.
/// </summary>
public sealed record VKContradictionArbitrationResult
{
    /// <summary>
    /// Gets the kind of contradiction detected.
    /// </summary>
    public required VKContradictionKind Kind { get; init; }

    /// <summary>
    /// Gets the ID of the target memory that is contradicted, if any.
    /// </summary>
    public string? ContradictedMemoryId { get; init; }

    /// <summary>
    /// Gets the revised/refined text for the fact, if applicable.
    /// </summary>
    public string? RefinedFact { get; init; }

    /// <summary>
    /// Gets the authority weight used during arbitration.
    /// </summary>
    public float AuthorityWeight { get; init; } = 0.7f;
}
