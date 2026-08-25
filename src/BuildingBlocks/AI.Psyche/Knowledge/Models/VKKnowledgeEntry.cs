using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents an entry in a knowledge/worldbook.
/// </summary>
public sealed record VKKnowledgeEntry : IVKFragmentMetadata
{
    /// <summary>
    /// Gets the unique identifier for the entry.
    /// </summary>
    public required VKKnowledgeId Id { get; init; }

    /// <summary>
    /// Gets the trigger activation strategy for this entry (e.g. Constant, Keyword, Regex).
    /// </summary>
    public VKKnowledgeTriggerType TriggerType { get; init; } = VKKnowledgeTriggerType.Constant;

    /// <summary>
    /// Gets the matching boolean evaluation logic when multiple keys are present (e.g. AndAny, AndAll).
    /// </summary>
    public VKKnowledgeFilterLogic FilterLogic { get; init; } = VKKnowledgeFilterLogic.AndAny;

    /// <summary>
    /// Gets the optional XML wrapper tag used when this entry is woven into the prompt.
    /// When null or whitespace, prompt assembly falls back to the default <c>knowledge</c> tag.
    /// Applies to both relative and absolute (pinned) positioning; use any custom string (e.g. <c>lore</c>, <c>important_knowledge</c>).
    /// </summary>
    public string? XmlTag { get; init; }

    /// <summary>
    /// Gets the structured keys that trigger this entry.
    /// </summary>
    public IReadOnlyList<VKKnowledgeKey> Keys { get; init; } = [];

    /// <summary>
    /// Gets the segment text for this knowledge entry.
    /// </summary>
    public required VKPromptSegment Segment { get; init; }

}
