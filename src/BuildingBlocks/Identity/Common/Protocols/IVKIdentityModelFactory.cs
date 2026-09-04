using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain model factory port for creating Identity aggregate roots and entities.
/// Standardized with Dual-Overload Pattern (Auto-generated Id & Explicit Id).
/// Binds <see cref="IVKGuidGenerator"/> and <see cref="TimeProvider"/> per CS.06.
/// Follows AP.01, AP.03.
/// </summary>
public interface IVKIdentityModelFactory
{
    // =========================================================================
    // 1. VKTenant (Aggregate Root) - Dual Overloads
    // =========================================================================

    /// <summary>
    /// Creates a new <see cref="VKTenant"/> aggregate root with auto-generated <see cref="VKTenantId"/>.
    /// </summary>
    VKResult<VKTenant> CreateTenant(
        string name,
        VKUserId ownerUserId,
        VKTenantPlan? plan = null,
        string? displayName = null,
        string? description = null,
        string? customDomain = null,
        string? externalId = null,
        DateTimeOffset? trialEndsAt = null,
        VKTenantSettings? settings = null);

    /// <summary>
    /// Creates a new <see cref="VKTenant"/> aggregate root with an explicit <see cref="VKTenantId"/>.
    /// </summary>
    VKResult<VKTenant> CreateTenant(
        VKTenantId id,
        string name,
        VKUserId ownerUserId,
        VKTenantPlan? plan = null,
        string? displayName = null,
        string? description = null,
        string? customDomain = null,
        string? externalId = null,
        DateTimeOffset? trialEndsAt = null,
        VKTenantSettings? settings = null);

    // =========================================================================
    // 2. VKUser (Aggregate Root) - Dual Overloads
    // =========================================================================

    /// <summary>
    /// Creates a new <see cref="VKUser"/> aggregate root with auto-generated <see cref="VKUserId"/>.
    /// </summary>
    VKResult<VKUser> CreateUser(
        VKEmail email,
        string? displayName = null,
        string? phoneNumber = null,
        string? avatarUrl = null,
        string? externalId = null,
        bool requireEmailVerification = false,
        VKUserSettings? settings = null);

    /// <summary>
    /// Creates a new <see cref="VKUser"/> aggregate root with an explicit <see cref="VKUserId"/>.
    /// </summary>
    VKResult<VKUser> CreateUser(
        VKUserId id,
        VKEmail email,
        string? displayName = null,
        string? phoneNumber = null,
        string? avatarUrl = null,
        string? externalId = null,
        bool requireEmailVerification = false,
        VKUserSettings? settings = null);

    // =========================================================================
    // 3. VKTenantUser (Domain Entity)
    // =========================================================================

    /// <summary>
    /// Creates a new <see cref="VKTenantUser"/> membership entity.
    /// </summary>
    VKResult<VKTenantUser> CreateTenantUser(
        VKTenantId tenantId,
        VKUserId userId,
        VKTenantRole role = VKTenantRole.Member,
        VKTenantMemberStatus status = VKTenantMemberStatus.Active,
        string? department = null,
        string? jobTitle = null,
        string? memberAlias = null,
        VKUserId? invitedBy = null);
}
