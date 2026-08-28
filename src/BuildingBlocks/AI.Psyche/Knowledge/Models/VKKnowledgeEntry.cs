using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain aggregate root representing an entry in a knowledge/worldbook.
/// Follows AP.01, CS.01.
/// </summary>
public sealed class VKKnowledgeEntry : VKAggregateRoot<VKKnowledgeId>, IVKFragmentMetadata
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the segment text for this knowledge entry.
    /// </summary>
    public VKPromptSegment Segment { get; private set; }

    /// <summary>
    /// Gets the trigger activation strategy for this entry (e.g. Constant, Keyword, Regex).
    /// </summary>
    public VKKnowledgeTriggerType TriggerType { get; private set; }

    /// <summary>
    /// Gets the matching boolean evaluation logic when multiple keys are present (e.g. AndAny, AndAll).
    /// </summary>
    public VKKnowledgeFilterLogic FilterLogic { get; private set; }

    /// <summary>
    /// Gets the optional XML wrapper tag used when this entry is woven into the prompt.
    /// </summary>
    public string? XmlTag { get; private set; }

    /// <summary>
    /// Gets the structured keys that trigger this entry.
    /// </summary>
    public IReadOnlyList<VKKnowledgeKey> Keys { get; private set; }

    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKKnowledgeEntry(
        VKKnowledgeId id,
        VKPromptSegment segment,
        VKKnowledgeTriggerType triggerType,
        VKKnowledgeFilterLogic filterLogic,
        string? xmlTag,
        IReadOnlyList<VKKnowledgeKey>? keys) : base(id)
    {
        Segment = segment;
        TriggerType = triggerType;
        FilterLogic = filterLogic;
        XmlTag = xmlTag;
        Keys = keys ?? [];
    }

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Factory method to create a new knowledge entry aggregate root.
    /// </summary>
    public static VKResult<VKKnowledgeEntry> Create(
        VKKnowledgeId id,
        VKPromptSegment segment,
        VKKnowledgeTriggerType triggerType = VKKnowledgeTriggerType.Constant,
        VKKnowledgeFilterLogic filterLogic = VKKnowledgeFilterLogic.AndAny,
        string? xmlTag = null,
        IReadOnlyList<VKKnowledgeKey>? keys = null)
    {
        // [AP.01]
        VKGuard.NotDefault(id);
        VKGuard.NotNull(segment);

        return VKResult.Success(new VKKnowledgeEntry(id, segment, triggerType, filterLogic, xmlTag, keys));
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKKnowledgeEntry Rehydrate(
        VKKnowledgeId id,
        VKPromptSegment segment,
        VKKnowledgeTriggerType triggerType,
        VKKnowledgeFilterLogic filterLogic,
        string? xmlTag,
        IReadOnlyList<VKKnowledgeKey>? keys)
    {
        return new VKKnowledgeEntry(id, segment, triggerType, filterLogic, xmlTag, keys);
    }

    // =========================================================================
    // Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Updates the prompt segment content and placement coordinates.
    /// </summary>
    public VKResult UpdateSegment(VKPromptSegment segment)
    {
        Segment = VKGuard.NotNull(segment);
        return VKResult.Success();
    }

    /// <summary>
    /// Updates trigger activation strategies and XML wrapping tag.
    /// </summary>
    public VKResult UpdateTriggerSettings(
        VKKnowledgeTriggerType triggerType,
        VKKnowledgeFilterLogic filterLogic,
        string? xmlTag)
    {
        TriggerType = triggerType;
        FilterLogic = filterLogic;
        XmlTag = xmlTag;
        return VKResult.Success();
    }

    /// <summary>
    /// Replaces the entire collection of trigger matching keys.
    /// </summary>
    public VKResult ReplaceKeys(IReadOnlyList<VKKnowledgeKey> keys)
    {
        Keys = VKGuard.NotNull(keys).ToList();
        return VKResult.Success();
    }

    /// <summary>
    /// Adds a single key to the knowledge entry keys collection.
    /// </summary>
    public VKResult AddKey(VKKnowledgeKey key)
    {
        VKGuard.NotNull(key);
        var list = new List<VKKnowledgeKey>(Keys) { key };
        Keys = list;
        return VKResult.Success();
    }
}
