namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Configurations tailored to specific corpus entries.
/// Follows BB.07.
/// </summary>
public sealed record VKKnowledgeLifecycle
{
    /// <summary>
    /// Gets the probability threshold (0.0 to 1.0) for randomized selection.
    /// </summary>
    public double Probability { get; init; } = 1.0;

    /// <summary>
    /// Gets the number of turns this entry remains active once triggered.
    /// </summary>
    public int? StickyTurns { get; init; }

    /// <summary>
    /// Gets the cooldown duration in turns before this entry can trigger again.
    /// </summary>
    public int? CooldownTurns { get; init; }

    /// <summary>
    /// Gets the number of turns to delay activation after being triggered.
    /// </summary>
    public int DelayTurns { get; init; } = VKKnowledgeLifecyclePresets.Delay.Immediate;

    /// <summary>
    /// Gets the exclusive group weight. When exclusive group pruning occurs, the highest weight survivor is kept.
    /// </summary>
    public int ExclusiveWeight { get; init; } = 100;

    /// <summary>
    /// Gets the group identifier for coordinating parent limits or mutually exclusive matches.
    /// </summary>
    public string? GroupId { get; init; }

    /// <summary>
    /// Gets the max number of session-wide usage limits for this entry.
    /// </summary>
    public int? MaxCount { get; init; }

    /// <summary>
    /// Gets the max number of injected entries allowed for this group in a single turn.
    /// </summary>
    public int? MaxCountPerTurn { get; init; }

    /// <summary>
    /// Gets the start turn range constraint.
    /// </summary>
    public int? StartTurn { get; init; }

    /// <summary>
    /// Gets the end turn range constraint.
    /// </summary>
    public int? EndTurn { get; init; }

    /// <summary>
    /// Gets the exclusion tag. If another entry with this tag has already been selected/injected, this entry is skipped.
    /// </summary>
    public string? ExclusionTag { get; init; }

    /// <summary>
    /// Gets the parent dependency ID. If defined, the parent entry must have been injected or active for this to fire.
    /// </summary>
    public string? DependencyId { get; init; }

    /// <summary>
    /// Gets the conflict group identifier. Mutually exclusive entries within the same group are resolved by priority.
    /// </summary>
    public string? ConflictGroupId { get; init; }

    /// <summary>
    /// Gets the minimum affection value required to inject this entry.
    /// </summary>
    public int? MinAffection { get; init; }

    /// <summary>
    /// Gets the maximum anger value allowed to inject this entry.
    /// </summary>
    public int? MaxAnger { get; init; }

    /// <summary>
    /// Gets the secret key required to be revealed (persistently) for this entry to be injected.
    /// </summary>
    public string? RevealSecretKey { get; init; }

    /// <summary>
    /// Gets the target Persona ID. If set, this entry is only injected for this specific Persona.
    /// </summary>
    public string? TargetPersonaId { get; init; }

    /// <summary>
    /// Gets the expiration timestamp for the entry.
    /// </summary>
    public System.DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Gets the allowed user segment (e.g. Free, Premium) constraint.
    /// </summary>
    public string? UserSegment { get; init; }
}
