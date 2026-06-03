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
    public required string Id { get; init; }

    /// <summary>
    /// Gets the XML wrapper tag used when this entry is woven into the prompt.
    /// Defaults to <see cref="VKKnowledgeXmlTags.Knowledge"/>.
    /// Applies to both relative and absolute (pinned) positioning; use any string (e.g. <c>lore</c>, <c>important_knowledge</c>).
    /// </summary>
    public string Tag { get; init; } = VKKnowledgeXmlTags.Knowledge;

    /// <summary>
    /// Gets a value indicating whether this entry is active and enabled for retrieval.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Gets the structured keys that trigger this entry.
    /// </summary>
    public IReadOnlyList<VKKnowledgeKey> Keys { get; init; } = [];

    /// <summary>
    /// Gets the content of the entry.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the target prompt template position where this entry should be woven.
    /// Defaults to <see cref="VKKnowledgeRelativePosition"/> at <see cref="VKKnowledgeRelative.AfterPersona"/>.
    /// </summary>
    public IVKKnowledgePosition Position { get; init; } = new VKKnowledgeRelativePosition(VKKnowledgeRelative.AfterPersona);

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
    /// Gets the rendering priority order of the entry when triggered.
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Gets the number of subsequent turns this entry remains active after being triggered.
    /// </summary>
    public int StickyTurns { get; init; } = 0;

    /// <summary>
    /// Gets the cooldown duration in turns before this entry can trigger again.
    /// </summary>
    public int CooldownTurns { get; init; } = 0;

    /// <summary>
    /// Gets the number of turns to delay activation after being triggered.
    /// </summary>
    public int DelayTurns { get; init; } = 0;

    /// <summary>
    /// Gets the exclusive grouping rule for this knowledge entry.
    /// </summary>
    public VKExclusiveGrouping? ExclusiveGrouping { get; init; }
}
