using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain aggregate root representing a SaaS Tenant / Organization within the identity system.
/// Follows AP.01, CS.01, CS.05.
/// </summary>
public sealed class VKTenant : VKAggregateRoot<VKTenantId>
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the unique tenant name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the custom domain.
    /// </summary>
    public string? CustomDomain { get; private set; }

    /// <summary>
    /// Gets the external mapping identifier (e.g. Stripe Customer ID, Auth0 Org ID).
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>
    /// Gets the lifecycle operational status.
    /// </summary>
    public VKTenantStatus Status { get; private set; }

    /// <summary>
    /// Gets the subscription plan tier name.
    /// </summary>
    public string PlanType { get; private set; }

    /// <summary>
    /// Gets the owner user identifier.
    /// </summary>
    public VKUserId OwnerUserId { get; private set; }

    /// <summary>
    /// Gets the trial expiration timestamp.
    /// </summary>
    public DateTimeOffset? TrialEndsAt { get; private set; }

    /// <summary>
    /// Gets tenant settings.
    /// </summary>
    public VKTenantSettings Settings { get; private set; }

    /// <summary>
    /// Gets tenant quota.
    /// </summary>
    public VKTenantQuota Quota { get; private set; }

    /// <summary>
    /// Gets the timestamp when the tenant was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the tenant was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Gets the structured subscription plan calculated from plan type and quota.
    /// </summary>
    public VKTenantPlan Plan => new(
        PlanType,
        Quota.MaxMembers,
        Quota.MaxStorageGb,
        TrialEndsAt);

    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKTenant(
        VKTenantId id,
        string name,
        string? displayName,
        string? description,
        string? customDomain,
        string? externalId,
        VKTenantStatus status,
        string planType,
        VKUserId ownerUserId,
        DateTimeOffset? trialEndsAt,
        VKTenantSettings? settings,
        VKTenantQuota? quota,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) : base(id)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        CustomDomain = customDomain;
        ExternalId = externalId;
        Status = status;
        PlanType = planType;
        OwnerUserId = ownerUserId;
        TrialEndsAt = trialEndsAt;
        Settings = settings ?? VKTenantSettings.Default;
        Quota = quota ?? VKTenantQuota.ForPlan(VKTenantPlan.Free);
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Factory method to create a new tenant aggregate root.
    /// </summary>
    public static VKResult<VKTenant> Create(
        VKTenantId id,
        string name,
        VKUserId ownerUserId,
        DateTimeOffset now,
        VKTenantPlan? plan = null,
        string? displayName = null,
        string? description = null,
        string? customDomain = null,
        string? externalId = null,
        DateTimeOffset? trialEndsAt = null,
        VKTenantSettings? settings = null)
    {
        VKGuard.NotDefault(id);
        VKGuard.NotNullOrWhiteSpace(name);
        VKGuard.NotDefault(ownerUserId);

        var initialPlan = plan ?? VKTenantPlan.Free;
        var initialStatus = trialEndsAt.HasValue ? VKTenantStatus.Trial : VKTenantStatus.Active;
        var quota = VKTenantQuota.ForPlan(initialPlan);

        var tenant = new VKTenant(
            id: id,
            name: name,
            displayName: displayName ?? name,
            description: description,
            customDomain: customDomain,
            externalId: externalId,
            status: initialStatus,
            planType: initialPlan.Name,
            ownerUserId: ownerUserId,
            trialEndsAt: trialEndsAt,
            settings: settings ?? VKTenantSettings.Default,
            quota: quota,
            createdAt: now,
            updatedAt: now);

        tenant.RaiseDomainEvent(new VKTenantCreatedEvent(id, name, ownerUserId, now));
        return VKResult.Success(tenant);
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKTenant Rehydrate(
        VKTenantId id,
        string name,
        string? displayName,
        string? description,
        string? customDomain,
        string? externalId,
        VKTenantStatus status,
        string planType,
        VKUserId ownerUserId,
        DateTimeOffset? trialEndsAt,
        VKTenantSettings settings,
        VKTenantQuota quota,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        return new VKTenant(
            id,
            name,
            displayName,
            description,
            customDomain,
            externalId,
            status,
            planType,
            ownerUserId,
            trialEndsAt,
            settings,
            quota,
            createdAt,
            updatedAt ?? createdAt);
    }

    // =========================================================================
    // Domain Invariants & Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Transfers tenant ownership to a new user.
    /// </summary>
    public VKResult TransferOwnership(VKUserId newOwnerUserId, DateTimeOffset now)
    {
        VKGuard.NotDefault(newOwnerUserId);

        if (Status == VKTenantStatus.Archived)
        {
            return VKResult.Failure(VKTenantErrors.TenantArchived);
        }

        if (OwnerUserId == newOwnerUserId)
        {
            return VKResult.Success();
        }

        var oldOwner = OwnerUserId;
        OwnerUserId = newOwnerUserId;
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantOwnerTransferredEvent(Id, oldOwner, newOwnerUserId, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Updates tenant display profile and routing metadata.
    /// </summary>
    public VKResult UpdateDetails(
        string name,
        string? displayName,
        string? description,
        string? customDomain,
        string? externalId,
        DateTimeOffset now)
    {
        VKGuard.NotNullOrWhiteSpace(name);

        if (Status == VKTenantStatus.Archived)
        {
            return VKResult.Failure(VKTenantErrors.TenantArchived);
        }

        Name = name;
        DisplayName = displayName ?? name;
        Description = description;
        CustomDomain = customDomain;
        ExternalId = externalId;
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantDetailsUpdatedEvent(Id, name, displayName, description, customDomain, externalId, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Changes the subscription plan and updates quotas accordingly.
    /// </summary>
    public VKResult ChangePlan(VKTenantPlan newPlan, DateTimeOffset now)
    {
        VKGuard.NotNull(newPlan);

        if (Status == VKTenantStatus.Archived)
        {
            return VKResult.Failure(VKTenantErrors.TenantArchived);
        }

        PlanType = newPlan.Name;
        Quota = VKTenantQuota.ForPlan(newPlan);
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantPlanChangedEvent(Id, newPlan, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Updates tenant settings.
    /// </summary>
    public VKResult UpdateSettings(VKTenantSettings settings, DateTimeOffset now)
    {
        VKGuard.NotNull(settings);

        if (Status == VKTenantStatus.Archived)
        {
            return VKResult.Failure(VKTenantErrors.TenantArchived);
        }

        Settings = settings;
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantSettingsUpdatedEvent(Id, settings, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Suspends the tenant account.
    /// </summary>
    public VKResult Suspend(string reason, DateTimeOffset now)
    {
        VKGuard.NotNullOrWhiteSpace(reason);

        if (Status == VKTenantStatus.Archived)
        {
            return VKResult.Failure(VKTenantErrors.TenantArchived);
        }

        Status = VKTenantStatus.Suspended;
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantSuspendedEvent(Id, reason, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Activates or re-activates the tenant account.
    /// </summary>
    public VKResult Activate(DateTimeOffset now)
    {
        if (Status == VKTenantStatus.Archived)
        {
            return VKResult.Failure(VKTenantErrors.TenantArchived);
        }

        if (Status == VKTenantStatus.Active)
        {
            return VKResult.Failure(VKTenantErrors.TenantAlreadyActive);
        }

        Status = VKTenantStatus.Active;
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantActivatedEvent(Id, now));
        return VKResult.Success();
    }

    /// <summary>
    /// Adds a user to the tenant with the specified role after verifying tenant invariants.
    /// </summary>
    public VKResult<VKTenantUser> AddUser(VKUserId userId, DateTimeOffset now, VKTenantRole role = VKTenantRole.Member)
    {
        if (Status == VKTenantStatus.Archived)
        {
            return VKResult.Failure<VKTenantUser>(VKTenantErrors.TenantArchived);
        }

        if (Status == VKTenantStatus.Suspended)
        {
            return VKResult.Failure<VKTenantUser>(VKTenantErrors.TenantSuspended);
        }

        return VKTenantUser.Create(Id, userId, now, role);
    }

    /// <summary>
    /// Permanently archives the tenant account.
    /// </summary>
    public VKResult Archive(DateTimeOffset now)
    {
        if (Status == VKTenantStatus.Archived)
        {
            return VKResult.Failure(VKTenantErrors.TenantAlreadyArchived);
        }

        Status = VKTenantStatus.Archived;
        UpdatedAt = now;

        RaiseDomainEvent(new VKTenantArchivedEvent(Id, now));
        return VKResult.Success();
    }
}
