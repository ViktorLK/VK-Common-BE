using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a VK AI Tenant Directive Charter.
/// Follows CS.05, CS.08.
/// </summary>
[VKPersistEntity(typeof(VKDirectiveCharter), TableName = "VK_AI_Psyche_Directive")]
public sealed class VKPsycheDirectiveEntity : IVKTenantScoped, IVKFullAuditable
{
    /// <inheritdoc />
    [VKPersistIndex]
    public VKTenantId TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique strongly-typed directive charter identifier.
    /// </summary>
    [VKPersistKey]
    public required VKDirectiveId Id { get; set; }

    /// <summary>
    /// Gets or sets behavioral boundaries and agent conduct rules.
    /// </summary>
    [MaxLength(4000)]
    public string? BehaviorRules { get; set; }

    /// <summary>
    /// Gets or sets safety guidelines, content moderation, and guardrails.
    /// </summary>
    [MaxLength(4000)]
    public string? SafetyRules { get; set; }

    /// <summary>
    /// Gets or sets output format requirements and response structure constraints.
    /// </summary>
    [MaxLength(4000)]
    public string? OutputConstraints { get; set; }

    /// <summary>
    /// Gets or sets high-level overview, role framing, and global objectives.
    /// </summary>
    [MaxLength(4000)]
    public string? Overview { get; set; }

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
