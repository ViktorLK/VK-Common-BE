using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.EFCore;

/// <summary>
/// Database persistence entity representing a Tenant-User relation / membership.
/// Follows CS.05, CS.08.
/// </summary>
[VKPersistEntity(typeof(VKTenantUser), TableName = "VK_Identity_TenantUser")]
public sealed class VKIdentityTenantUserEntity : IVKFullAuditable
{
    /// <summary>
    /// Gets or sets the composite primary key part 1: TenantId.
    /// </summary>
    [VKPersistKey(Order = 1)]
    public required VKTenantId TenantId { get; set; }

    /// <summary>
    /// Gets or sets the composite primary key part 2: UserId.
    /// </summary>
    [VKPersistKey(Order = 2)]
    public required VKUserId UserId { get; set; }

    /// <summary>
    /// Gets or sets the tenant membership role.
    /// </summary>
    public byte Role { get; set; }

    /// <summary>
    /// Gets or sets the tenant membership lifecycle status.
    /// </summary>
    public byte Status { get; set; }

    /// <summary>
    /// Gets or sets the department name within this tenant.
    /// </summary>
    [MaxLength(128)]
    public string? Department { get; set; }

    /// <summary>
    /// Gets or sets the job title within this tenant.
    /// </summary>
    [MaxLength(128)]
    public string? JobTitle { get; set; }

    /// <summary>
    /// Gets or sets the member nickname/alias within this tenant.
    /// </summary>
    [MaxLength(128)]
    public string? MemberAlias { get; set; }

    /// <summary>
    /// Gets or sets the inviter user identifier.
    /// </summary>
    [VKPersistIndex]
    public VKUserId? InvitedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the user joined the tenant.
    /// </summary>
    public DateTimeOffset JoinedAt { get; set; }

    // =========================================================================
    // Full Auditing & Soft Delete (CS.05)
    // =========================================================================

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public VKUserId? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public VKUserId? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public VKUserId? DeletedBy { get; set; }
}
