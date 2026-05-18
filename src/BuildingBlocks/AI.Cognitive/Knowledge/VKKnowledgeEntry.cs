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
    /// Gets the structured keys that trigger this entry.
    /// </summary>
    public IReadOnlyList<VKKnowledgeKey> Keys { get; init; } = [];

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

    /// <summary>
    /// Gets the optional developer memo or description about this entry.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    /// Gets the rendering priority order of the entry when triggered.
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Gets the target prompt template position where this entry should be woven.
    /// Defaults to "BeforeDefs".
    /// </summary>
    public string Position { get; init; } = "BeforeDefs";

    /// <summary>
    /// Gets the prompt insertion depth index. 
    /// Determines the woven proximity relative to prompt borders. Defaults to 0.
    /// </summary>
    public int Depth { get; init; } = 0;

    /// <summary>
    /// Gets the target chat role for this knowledge entry in the conversation prompt.
    /// Defaults to "system".
    /// </summary>
    public string Role { get; init; } = "system";

    /// <summary>
    /// Gets a value indicating whether this entry is active and enabled for retrieval.
    /// </summary>
    public bool IsEnabled { get; init; } = true;
}
