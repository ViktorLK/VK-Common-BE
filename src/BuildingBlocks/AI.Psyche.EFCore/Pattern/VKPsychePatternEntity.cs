using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a Pattern Entry (Few-Shot pattern/schema).
/// Follows CS.05, CS.08.
/// </summary>
[VKPersistEntity(typeof(VKPatternEntry), TableName = "VK_AI_Psyche_Pattern", FlattenBy = [nameof(VKPatternEntry.Segment)])]
public sealed class VKPsychePatternEntity : IVKTenantScoped, IVKFullAuditable
{
    /// <inheritdoc />
    [VKPersistIndex]
    public VKTenantId TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique strongly-typed pattern identifier.
    /// </summary>
    [VKPersistKey]
    public required VKPatternId Id { get; set; }

    /// <summary>
    /// Gets or sets the few-shot template or response schema prompt content.
    /// </summary>
    [Required]
    [MaxLength(16000)]
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the optional human-readable name of this pattern entry.
    /// </summary>
    [MaxLength(128)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this pattern is active.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the target chat role when this pattern is rendered in prompt context.
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
