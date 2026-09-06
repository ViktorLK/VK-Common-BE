using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain aggregate root representing the relation and membership of a user within a specific tenant.
/// Follows AP.01, CS.01, CS.05.
/// </summary>
public sealed class VKTenantUser : VKAggregateRoot
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public VKTenantId TenantId { get; private set; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public VKUserId UserId { get; private set; }

    /// <summary>
    /// Gets the tenant membership role.
    /// </summary>
    public VKTenantRole Role { get; private set; }

    /// <summary>
    /// Gets the tenant membership lifecycle status.
    /// </summary>
    public VKTenantMemberStatus Status { get; private set; }

    /// <summary>
    /// Gets the department name within this tenant.
    /// </summary>
    public string? Department { get; private set; }

    /// <summary>
    /// Gets the job title within this tenant.
    /// </summary>
    public string? JobTitle { get; private set; }

    /// <summary>
    /// Gets the member nickname/alias within this tenant.
    /// </summary>
    public string? MemberAlias { get; private set; }

    /// <summary>
    /// Gets the inviter user identifier.
    /// </summary>
    public VKUserId? InvitedBy { get; private set; }

    /// <summary>
    /// Gets the timestamp when the user joined the tenant.
    /// </summary>
    public DateTimeOffset JoinedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the relation was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the relation was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKTenantUser(
        VKTenantId tenantId,
        VKUserId userId,
        VKTenantRole role,
        VKTenantMemberStatus status,
        string? department,
        string? jobTitle,
        string? memberAlias,
        VKUserId? invitedBy,
        DateTimeOffset joinedAt,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        TenantId = tenantId;
        UserId = userId;
        Role = role;
        Status = status;
        Department = department;
        JobTitle = jobTitle;
        MemberAlias = memberAlias;
        InvitedBy = invitedBy;
        JoinedAt = joinedAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Factory method to establish a new tenant-user relation aggregate root.
    /// </summary>
    public static VKResult<VKTenantUser> Create(
        VKTenantId tenantId,
        VKUserId userId,
        DateTimeOffset now,
        VKTenantRole role = VKTenantRole.Member,
        VKTenantMemberStatus status = VKTenantMemberStatus.Active,
        string? department = null,
        string? jobTitle = null,
        string? memberAlias = null,
        VKUserId? invitedBy = null)
    {
        if (tenantId == VKTenantId.Default)
        {
            return VKResult.Failure<VKTenantUser>(VKTenantUserErrors.InvalidTenantId);
        }

        if (userId == VKUserId.Anonymous)
        {
            return VKResult.Failure<VKTenantUser>(VKTenantUserErrors.InvalidUserId);
        }

        var tenantUser = new VKTenantUser(
            tenantId: tenantId,
            userId: userId,
            role: role,
            status: status,
            department: department,
            jobTitle: jobTitle,
            memberAlias: memberAlias,
            invitedBy: invitedBy,
            joinedAt: now,
            createdAt: now,
            updatedAt: now);

        tenantUser.RaiseDomainEvent(new VKUserJoinedTenantEvent(tenantId, userId, role, now));
        return VKResult.Success(tenantUser);
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKTenantUser Rehydrate(
        VKTenantId tenantId,
        VKUserId userId,
        VKTenantRole role,
        VKTenantMemberStatus status,
        string? department,
        string? jobTitle,
        string? memberAlias,
        VKUserId? invitedBy,
        DateTimeOffset joinedAt,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        return new VKTenantUser(
            tenantId,
            userId,
            role,
            status,
            department,
            jobTitle,
            memberAlias,
            invitedBy,
            joinedAt,
            createdAt,
            updatedAt ?? createdAt);
    }

    // =========================================================================
    // Domain Invariants & Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Changes the role of this user within the tenant.
    /// </summary>
    public VKResult ChangeRole(VKTenantRole newRole, DateTimeOffset now)
    {
        if (Role == newRole)
        {
            return VKResult.Success();
        }

        var oldRole = Role;
        Role = newRole;
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantUserRoleChangedEvent(TenantId, UserId, oldRole, newRole, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Updates tenant-scoped job title, department, and member alias.
    /// </summary>
    public VKResult UpdateProfile(
        string? department,
        string? jobTitle,
        string? memberAlias,
        DateTimeOffset now)
    {
        Department = department;
        JobTitle = jobTitle;
        MemberAlias = memberAlias;
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantUserProfileUpdatedEvent(TenantId, UserId, department, jobTitle, memberAlias, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Activates an invited or suspended member.
    /// </summary>
    public VKResult Activate(DateTimeOffset now)
    {
        Status = VKTenantMemberStatus.Active;
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantUserActivatedEvent(TenantId, UserId, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Suspends a member's access in this tenant.
    /// </summary>
    public VKResult Suspend(DateTimeOffset now)
    {
        Status = VKTenantMemberStatus.Suspended;
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantUserSuspendedEvent(TenantId, UserId, now));
        return VKResult.Success();
    }
}
