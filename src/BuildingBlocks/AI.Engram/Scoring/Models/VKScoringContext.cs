using System.Collections.Generic;
using System.Collections.Frozen;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Execution context for IVKScoringStrategy allowing Persona and Category differentiation.
/// </summary>
public sealed record VKScoringContext
{
    /// <summary>
    /// Gets the raw text content of the memory entry.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the category of the memory entry (ShortTerm, MediumTerm, LongTerm).
    /// </summary>
    public VKMemoryCategory Category { get; init; } = VKMemoryCategory.ShortTerm;

    /// <summary>
    /// Gets the optional persona identifier.
    /// </summary>
    public string? PersonaId { get; init; }

    /// <summary>
    /// Gets the optional emotional signal.
    /// </summary>
    public VKEmotionalSignal? Emotion { get; init; }

    /// <summary>
    /// Gets the metadata associated with the memory entry.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = FrozenDictionary<string, string>.Empty;
}
