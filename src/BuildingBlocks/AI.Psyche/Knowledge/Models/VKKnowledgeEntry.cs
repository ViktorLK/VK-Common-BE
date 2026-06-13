using System.Collections.Generic;
using VK.Blocks.AI.Psyche.Common.Internal;

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
    /// Gets the XML wrapper tag used when this entry is woven into the prompt.
    /// Defaults to <see cref="VKKnowledgeXmlTags.Knowledge"/>.
    /// Applies to both relative and absolute (pinned) positioning; use any string (e.g. <c>lore</c>, <c>important_knowledge</c>).
    /// </summary>
    public string Tag { get; init; } = PsycheConstants.XmlTags.Knowledge;

    /// <summary>
    /// Gets the structured keys that trigger this entry.
    /// </summary>
    public IReadOnlyList<VKKnowledgeKey> Keys { get; init; } = [];

    /// <summary>
    /// Gets the layout segment coordinates of the entry.
    /// </summary>
    public required VKPromptSegment Segment { get; init; }

    /// <summary>
    /// Gets the trigger type.
    /// </summary>
    public VKKnowledgeTriggerType TriggerType { get; init; } = VKKnowledgeTriggerType.Keyword;

    /// <summary>
    /// Gets the logical combination rules used when matching multiple trigger keys.
    /// Defaults to <see cref="VKKnowledgeFilterLogic.AndAny"/>.
    /// </summary>
    public VKKnowledgeFilterLogic FilterLogic { get; init; } = VKKnowledgeFilterLogic.AndAny;

    /// <summary>
    /// Gets the generic payload for engine-specific extensions (e.g. JSON conditions, metadata).
    /// </summary>
    public string? Payload { get; init; }
}
