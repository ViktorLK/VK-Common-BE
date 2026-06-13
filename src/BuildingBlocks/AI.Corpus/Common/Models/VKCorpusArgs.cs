using System;
using System.Collections.Generic;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Execution/request-specific arguments for the AI.Corpus block.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKCorpusArgs
{
    /// <summary>
    /// Gets the current user segment constraint.
    /// </summary>
    public string? UserSegment { get; init; }

    /// <summary>
    /// Gets the current state values (e.g. affection, anger) for emotion gating.
    /// </summary>
    public IReadOnlyDictionary<string, int> StateValues { get; init; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the set of unlocked secret keys.
    /// </summary>
    public IReadOnlySet<string> UnlockedSecrets { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
