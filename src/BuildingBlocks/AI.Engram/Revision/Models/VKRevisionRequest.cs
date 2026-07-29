using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Explicit contract payload for triggering a memory revision operation.
/// </summary>
public sealed record VKRevisionRequest
{
    /// <summary>
    /// Gets the unique memory ID targeted for revision, if known.
    /// </summary>
    public VKMemoryId? TargetMemoryId { get; init; }

    /// <summary>
    /// Gets the content/fact for revision.
    /// </summary>
    public required string FactContent { get; init; }

    /// <summary>
    /// Gets the source classification of this revision signal.
    /// </summary>
    public VKRevisionSourceType SourceType { get; init; } = VKRevisionSourceType.LLMInferred;

    /// <summary>
    /// Gets the authority weight for conflict arbitration (0.0 to 1.0).
    /// </summary>
    public float AuthorityWeight { get; init; } = 0.7f;

    /// <summary>
    /// Gets an optional client mutation ID used for idempotency enforcement.
    /// </summary>
    public string? MutationId { get; init; }

    /// <summary>
    /// Gets optional extended metadata context for audit logging.
    /// </summary>
    public IReadOnlyDictionary<string, string> ContextMetadata { get; init; } = new Dictionary<string, string>();
}
