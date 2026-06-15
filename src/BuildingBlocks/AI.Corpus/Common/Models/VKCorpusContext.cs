using System.Collections.Generic;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Execution/runtime context containing state information referenced during corpus filtering.
/// </summary>
public sealed record VKCorpusContext
{
    /// <summary>
    /// Gets the current session identifier.
    /// </summary>
    public required VKSessionId SessionId { get; init; }

    /// <summary>
    /// Gets the current conversation turn.
    /// </summary>
    public required int CurrentTurn { get; init; }

    /// <summary>
    /// Gets the set of tags that have already been injected in the current workflow.
    /// </summary>
    public IReadOnlySet<string> InjectedTags { get; init; } = new HashSet<string>();

    /// <summary>
    /// Gets the total number of times specific entries (by Key or Tag) have been injected in this session.
    /// </summary>
    public IReadOnlyDictionary<string, int> UsageCounts { get; init; } = new Dictionary<string, int>();

    /// <summary>
    /// Gets the last turn number at which specific entries (by Key or Tag) were injected.
    /// </summary>
    public IReadOnlyDictionary<string, int> LastInjectedTurns { get; init; } = new Dictionary<string, int>();

    /// <summary>
    /// Gets the current persona identifier.
    /// </summary>
    public string? PersonaId { get; init; }

    /// <summary>
    /// Gets the current user segment (e.g. Free, Premium).
    /// </summary>
    public string? UserSegment { get; init; }

    /// <summary>
    /// Gets the current state values (e.g. affection, anger) for emotion gating.
    /// </summary>
    public IReadOnlyDictionary<string, int> StateValues { get; init; } = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the set of unlocked secret keys.
    /// </summary>
    public IReadOnlySet<string> UnlockedSecrets { get; init; } = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the list of dialogue history and input texts to scan for keyword matching.
    /// </summary>
    public IReadOnlyList<string> ScanTexts { get; init; } = [];
}
