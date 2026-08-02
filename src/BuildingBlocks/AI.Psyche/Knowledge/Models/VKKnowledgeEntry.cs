using System.Collections.Generic;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents an entry in a knowledge/worldbook. Implements <see cref="IVKTenantScoped"/>.
/// Order follows TenantId -> Id hierarchy.
/// </summary>
public sealed record VKKnowledgeEntry : IVKFragmentMetadata, IVKTenantScoped
{
    /// <summary>
    /// Gets the tenant identifier for multi-tenant SaaS isolation. Defaults to <see cref="VKTenantId.Default"/>.
    /// </summary>
    public VKTenantId TenantId { get; init; } = VKTenantId.Default;

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
    /// Gets the XML wrapper tag used when this entry is woven into the prompt.
    /// Defaults to <see cref="VKKnowledgeXmlTags.Knowledge"/>.
    /// Applies to both relative and absolute (pinned) positioning; use any string (e.g. <c>lore</c>, <c>important_knowledge</c>).
    /// </summary>
    public string XmlTag { get; init; } = PsycheConstants.XmlTags.Knowledge;

    /// <summary>
    /// Gets the structured keys that trigger this entry.
    /// </summary>
    public IReadOnlyList<VKKnowledgeKey> Keys { get; init; } = [];

    /// <summary>
    /// Gets the segment text for this knowledge entry.
    /// </summary>
    public required VKPromptSegment Segment { get; init; }
}
