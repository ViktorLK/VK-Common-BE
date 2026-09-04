using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.Common.Internal;

/// <summary>
/// Default implementation of <see cref="IVKIdentityModelFactory"/> binding <see cref="IVKGuidGenerator"/> and <see cref="TimeProvider"/>.
/// Standardized with Dual-Overload Pattern (Auto-generated Id & Explicit Id) modeled after Psyche factory.
/// Follows AP.01, CS.06.
/// </summary>
internal sealed class DefaultIdentityModelFactory(
    IVKGuidGenerator guidGenerator,
    TimeProvider timeProvider) : IVKIdentityModelFactory
{
    private readonly IVKGuidGenerator _guidGenerator = VKGuard.NotNull(guidGenerator);
    private readonly TimeProvider _timeProvider = VKGuard.NotNull(timeProvider);

    // =========================================================================
    // 1. VKTenant (Aggregate Root)
    // =========================================================================

    /// <inheritdoc />
    public VKResult<VKTenant> CreateTenant(
        string name,
        VKUserId ownerUserId,
        VKTenantPlan? plan = null,
        string? displayName = null,
        string? description = null,
        string? customDomain = null,
        string? externalId = null,
        DateTimeOffset? trialEndsAt = null,
        VKTenantSettings? settings = null)
    {
        var tenantId = new VKTenantId(_guidGenerator.Create());
        return CreateTenant(tenantId, name, ownerUserId, plan, displayName, description, customDomain, externalId, trialEndsAt, settings);
    }

    /// <inheritdoc />
    public VKResult<VKTenant> CreateTenant(
        VKTenantId id,
        string name,
        VKUserId ownerUserId,
        VKTenantPlan? plan = null,
        string? displayName = null,
        string? description = null,
        string? customDomain = null,
        string? externalId = null,
        DateTimeOffset? trialEndsAt = null,
        VKTenantSettings? settings = null)
    {
        var now = _timeProvider.GetUtcNow();

        return VKTenant.Create(
            id: id,
            name: name,
            ownerUserId: ownerUserId,
            now: now,
            plan: plan,
            displayName: displayName,
            description: description,
            customDomain: customDomain,
            externalId: externalId,
            trialEndsAt: trialEndsAt,
            settings: settings);
    }

    // =========================================================================
    // 2. VKUser (Aggregate Root)
    // =========================================================================

    /// <inheritdoc />
    public VKResult<VKUser> CreateUser(
        VKEmail email,
        string? displayName = null,
        string? phoneNumber = null,
        string? avatarUrl = null,
        string? externalId = null,
        bool requireEmailVerification = false,
        VKUserSettings? settings = null)
    {
        var userId = new VKUserId(_guidGenerator.Create());
        return CreateUser(userId, email, displayName, phoneNumber, avatarUrl, externalId, requireEmailVerification, settings);
    }

    /// <inheritdoc />
    public VKResult<VKUser> CreateUser(
        VKUserId id,
        VKEmail email,
        string? displayName = null,
        string? phoneNumber = null,
        string? avatarUrl = null,
        string? externalId = null,
        bool requireEmailVerification = false,
        VKUserSettings? settings = null)
    {
        var now = _timeProvider.GetUtcNow();

        return VKUser.Create(
            id: id,
            email: email,
            now: now,
            displayName: displayName,
            phoneNumber: phoneNumber,
            avatarUrl: avatarUrl,
            externalId: externalId,
            requireEmailVerification: requireEmailVerification,
            settings: settings);
    }

    // =========================================================================
    // 3. VKTenantUser (Domain Entity)
    // =========================================================================

    /// <inheritdoc />
    public VKResult<VKTenantUser> CreateTenantUser(
        VKTenantId tenantId,
        VKUserId userId,
        VKTenantRole role = VKTenantRole.Member,
        VKTenantMemberStatus status = VKTenantMemberStatus.Active,
        string? department = null,
        string? jobTitle = null,
        string? memberAlias = null,
        VKUserId? invitedBy = null)
    {
        var now = _timeProvider.GetUtcNow();

        return VKTenantUser.Create(
            tenantId: tenantId,
            userId: userId,
            now: now,
            role: role,
            status: status,
            department: department,
            jobTitle: jobTitle,
            memberAlias: memberAlias,
            invitedBy: invitedBy);
    }
}
