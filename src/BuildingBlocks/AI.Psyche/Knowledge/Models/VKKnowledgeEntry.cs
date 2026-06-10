using System.Collections.Generic;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;

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
    /// Defaults to <see cref="VKRelativePromptPosition"/> at <see cref="VKPromptRelativeAnchor.AfterPersona"/>.
    /// </summary>
    public IVKPromptPosition Position { get; init; } = new VKRelativePromptPosition(VKPromptRelativeAnchor.AfterPersona);

    /// <summary>
    /// Gets the trigger type.
    /// </summary>
    public VKKnowledgeTriggerType TriggerType { get; init; } = VKKnowledgeTriggerType.Keyword;

    /// <summary>
    /// Gets the logical combination rules used when matching multiple trigger keys.
    /// Defaults to <see cref="VKKnowledgeFilterLogic.AndAny"/>.
    /// </summary>
    public VKKnowledgeFilterLogic FilterLogic { get; init; } = VKKnowledgeFilterLogic.AndAny;

    private readonly int _priority = 0;

    /// <summary>
    /// Gets the rendering priority order of the entry when triggered.
    /// Priority must be between 0 and 999.
    /// </summary>
    public int Priority
    {
        get => _priority;
        init => _priority = VKGuard.InRange(value, 0, 999, nameof(Priority));
    }

    /// <summary>
    /// Gets the maximum number of elapsed turns this entry remains active after being triggered.
    /// Calculated dynamically: remains active as long as Elapsed &lt;= (DelayTurns + StickyTurns).
    /// Use <see cref="VKKnowledgeLifecycles.Sticky"/> presets for standard behaviors.
    /// </summary>
    public int StickyTurns { get; init; } = VKKnowledgeLifecycles.Sticky.Flash;

    /// <summary>
    /// Gets the cooldown duration in turns before this entry can trigger again.
    /// Blocks the keyword matcher for this many turns after a successful trigger.
    /// Example: If triggered at turn 1, and Cooldown = 5, it cannot be triggered again until turn 6.
    /// </summary>
    public int CooldownTurns { get; init; } = VKKnowledgeLifecycles.Cooldown.None;

    /// <summary>
    /// Gets the number of turns to delay activation after being triggered.
    /// The knowledge becomes active when Elapsed &gt;= DelayTurns, and stays active until Elapsed &lt;= (DelayTurns + StickyTurns).
    /// </summary>
    public int DelayTurns { get; init; } = VKKnowledgeLifecycles.Delay.Immediate;

    /// <summary>
    /// Gets the exclusive grouping rule for this knowledge entry.
    /// </summary>
    public VKExclusiveGrouping? ExclusiveGrouping { get; init; }

    /// <summary>
    /// Gets the generic payload for engine-specific extensions (e.g. JSON conditions, metadata).
    /// </summary>
    public string? Payload { get; init; }
}
