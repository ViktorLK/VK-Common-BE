using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents an entry in a knowledge/worldbook.
/// </summary>
public sealed record VKKnowledgeEntry
{
    /// <summary>
    /// Gets the unique identifier for the entry.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the keys that trigger this entry.
    /// </summary>
    public IEnumerable<string> Keys { get; init; } = [];

    /// <summary>
    /// Gets the content of the entry.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the trigger type.
    /// </summary>
    public VKKnowledgeTriggerType TriggerType { get; init; } = VKKnowledgeTriggerType.Keyword;

    /// <summary>
    /// Gets the weight of this entry for conflict resolution.
    /// Higher weight entries take precedence.
    /// </summary>
    public int Weight { get; init; } = 0;
}
