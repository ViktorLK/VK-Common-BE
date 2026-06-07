using VK.Blocks.AI.Cognitive;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents the narrative and turn-based activation rules component (attachment)
/// associated with a generic knowledge entry.
/// Follows AP.01 (Sealed Record) and AP.03.
/// </summary>
public sealed record VKKnowledgeNarrativeRules
{
    /// <summary>
    /// Gets the foreign key referencing the core <see cref="VKKnowledgeEntry.Id"/>.
    /// <remarks>
    /// DB Mapping: Acts as both the Primary Key and Foreign Key in the narrative store table,
    /// establishing a stateful 1:1 relationship with the factual entry.
    /// </remarks>
    /// </summary>
    public required string KnowledgeId { get; init; }

    /// <summary>
    /// Gets the number of subsequent turns this entry remains active after being triggered.
    /// </summary>
    public int StickyTurns { get; init; } = 0;

    /// <summary>
    /// Gets the cooldown duration in turns before this entry can trigger again.
    /// </summary>
    public int CooldownTurns { get; init; } = 0;

    /// <summary>
    /// Gets the delayed activation offset in turns after the keywords match.
    /// </summary>
    public int DelayTurns { get; init; } = 0;

    /// <summary>
    /// Gets the optional mutual exclusion group name.
    /// Triggering multiple entries from the same group triggers competitive pruning.
    /// </summary>
    public string? InclusionGroup { get; init; }

    /// <summary>
    /// Gets the competitive priority weight within the mutual exclusion group.
    /// Higher weight wins conflicts. Defaults to 100.
    /// </summary>
    public int GroupWeight { get; init; } = 100;

    /// <summary>
    /// Gets the trigger likelihood percentage (0-100).
    /// Entry will only activate if a random roll is within this threshold. Defaults to 100.
    /// </summary>
    public int Probability { get; init; } = 100;
}
