using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a Knowledge Entry in Psyche.
/// Follows CS.05, CS.08.
/// </summary>
[VKPersistEntity(
    typeof(VKKnowledgeEntry),
    TableName = "VK_AI_Psyche_Knowledge",
    FlattenBy = [nameof(VKKnowledgeEntry.Segment)],
    ProjectBy = [nameof(VKKnowledgeEntry.Keys)])]
public sealed class VKPsycheKnowledgeEntity : IVKTenantScoped, IVKFullAuditable
{
    /// <inheritdoc />
    [VKPersistIndex(Group = "Tenant_Trigger", Order = 1)]
    public VKTenantId TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique strongly-typed knowledge entry identifier.
    /// </summary>
    [VKPersistKey]
    public required VKKnowledgeId Id { get; set; }

    /// <summary>
    /// Gets or sets the main knowledge text content or fact snippet.
    /// </summary>
    [Required]
    [MaxLength(16000)]
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the optional human-readable name of this knowledge card.
    /// </summary>
    [MaxLength(128)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this knowledge entry is currently active.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the target chat role when this knowledge is rendered in prompt context.
    /// </summary>
    public VKChatRole Role { get; set; } = VKChatRole.System;

    /// <summary>
    /// Gets or sets the absolute position depth in prompt assembly if specified.
    /// </summary>
    public int? AbsoluteDepth { get; set; }

    /// <summary>
    /// Gets or sets the relative position section (e.g. SystemTop, ContextAfter, UserBefore).
    /// </summary>
    public VKPromptRelativeDepth? RelativeDepth { get; set; }

    /// <summary>
    /// Gets or sets the tie-breaking priority when multiple segments share the same position.
    /// </summary>
    public int DepthPriority { get; set; }

    /// <summary>
    /// Gets or sets the activation trigger type (e.g. Constant, Keyword, Semantic).
    /// </summary>
    [VKPersistIndex(Group = "Tenant_Trigger", Order = 2)]
    public VKKnowledgeTriggerType TriggerType { get; set; } = VKKnowledgeTriggerType.Constant;

    /// <summary>
    /// Gets or sets the key matching filter logic (AndAny, AndAll, Exact).
    /// </summary>
    public VKKnowledgeFilterLogic FilterLogic { get; set; } = VKKnowledgeFilterLogic.AndAny;

    /// <summary>
    /// Gets or sets the optional XML wrapper tag name when injected into prompt context.
    /// </summary>
    [MaxLength(64)]
    public string? XmlTag { get; set; }

    /// <summary>
    /// Gets or sets the collection of trigger keywords / matching keys.
    /// </summary>
    public ICollection<VKPsycheKnowledgeKeyEntity> Keys { get; set; } = [];

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? UpdatedBy { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? DeletedBy { get; set; }
}
