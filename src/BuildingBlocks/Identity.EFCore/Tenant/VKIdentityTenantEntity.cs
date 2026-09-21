using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.EFCore;

/// <summary>
/// Database persistence entity representing a SaaS Tenant / Organization within the identity system.
/// Follows CS.05, CS.08.
/// </summary>
[VKPersistEntity(typeof(VKTenant), TableName = "VK_Identity_Tenant", FlattenBy = ["Settings", "Quota"])]
public sealed class VKIdentityTenantEntity : IVKFullAuditable
{
    /// <summary>
    /// Gets or sets the strongly-typed tenant identifier.
    /// </summary>
    [VKPersistKey]
    public required VKTenantId Id { get; set; }

    /// <summary>
    /// Gets or sets the unique tenant name.
    /// </summary>
    [VKPersistIndex(IsUnique = true)]
    [MaxLength(128)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [MaxLength(128)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [MaxLength(512)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the custom domain.
    /// </summary>
    [VKPersistIndex(IsUnique = true)]
    [MaxLength(256)]
    public string? CustomDomain { get; set; }

    /// <summary>
    /// Gets or sets the external mapping identifier (e.g. Stripe Customer ID, Auth0 Org ID).
    /// </summary>
    [VKPersistIndex]
    [MaxLength(128)]
    public string? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the lifecycle operational status.
    /// </summary>
    public byte Status { get; set; }

    /// <summary>
    /// Gets or sets the subscription plan tier name.
    /// </summary>
    [MaxLength(64)]
    public required string PlanType { get; set; }

    /// <summary>
    /// Gets or sets the owner user identifier.
    /// </summary>
    [VKPersistIndex]
    public required VKUserId OwnerUserId { get; set; }

    /// <summary>
    /// Gets or sets the trial expiration timestamp.
    /// </summary>
    public DateTimeOffset? TrialEndsAt { get; set; }

    // =========================================================================
    // Value Object Flattened Properties (Settings & Quota)
    // =========================================================================

    public int MaxMembers { get; set; }
    public int MaxStorageGb { get; set; }
    public int MaxApiTokens { get; set; }
    public bool AllowCrossTenantInvite { get; set; }
    public bool RequireMfa { get; set; }

    [MaxLength(64)]
    public string? TimeZone { get; set; }

    [MaxLength(32)]
    public string? DefaultLanguage { get; set; }

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
