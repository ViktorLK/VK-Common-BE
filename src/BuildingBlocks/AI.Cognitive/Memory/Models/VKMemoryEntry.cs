using System;
using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Infrastructure Persistence DTO: Represents a static entry in the AI Reality Ledger.
/// <para>
/// This is the lowest-level physical storage model. Unlike a <see cref="VKMemoryTrace"/>, 
/// an Entry is mathematically cold—it does not decay and has no biological activation. 
/// It serves purely as the immutable, objective audit log of what actually happened.
/// </para>
/// </summary>
public sealed record VKMemoryEntry
{
    /// <summary>
    /// Gets the unique identifier for the memory entry.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the content/text of the memory.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the metadata associated with the memory.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the timestamp when the memory was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the category of the memory, determining its life-cycle strategy.
    /// </summary>
    public VKMemoryCategory Category { get; init; } = VKMemoryCategory.ShortTerm;

    /// <summary>
    /// Gets the importance score of the memory (0.0 to 1.0).
    /// </summary>
    public float Importance { get; init; } = 1.0f;

    /// <summary>
    /// Gets the timestamp when the memory was last accessed or retrieved.
    /// </summary>
    public DateTimeOffset? LastAccessedAt { get; init; }
}
