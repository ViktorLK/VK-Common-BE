using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.EFCore;

/// <summary>
/// Database persistence entity representing a User within the identity system.
/// Follows CS.05, CS.08.
/// </summary>
[VKPersistEntity(typeof(VKUser), TableName = "VK_Identity_User", FlattenBy = ["Settings"])]
public sealed class VKIdentityUserEntity : IVKFullAuditable
{
    /// <summary>
    /// Gets or sets the strongly-typed user identifier.
    /// </summary>
    [VKPersistKey]
    public required VKUserId Id { get; set; }

    /// <summary>
    /// Gets or sets the unique email address.
    /// </summary>
    [VKPersistIndex(IsUnique = true)]
    [MaxLength(256)]
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [MaxLength(128)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the contact phone number.
    /// </summary>
    [MaxLength(32)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the avatar image URL.
    /// </summary>
    [MaxLength(1024)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the external IdP identity identifier (e.g. Auth0, Google sub).
    /// </summary>
    [VKPersistIndex]
    [MaxLength(128)]
    public string? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the user account status.
    /// </summary>
    public byte Status { get; set; }

    public bool IsEmailConfirmed { get; set; }
    public bool IsPhoneNumberConfirmed { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    // =========================================================================
    // User Settings Flattened Properties
    // =========================================================================

    [MaxLength(32)]
    public string? PreferredLanguage { get; set; }

    [MaxLength(64)]
    public string? TimeZone { get; set; }

    [MaxLength(32)]
    public string? Theme { get; set; }

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
