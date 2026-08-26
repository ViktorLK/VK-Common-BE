using System;
using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Core;

/// <summary>
/// Unified immutable composite execution context envelope residing in the ambient async flow (SSoT).
/// Combines independent spatial (<see cref="IVKTenantCoordinate"/>) and subject (<see cref="IVKUserCoordinate"/>) slots.
/// Follows AP.01, AP.03, AP.06, CS.01.
/// </summary>
public sealed record VKExecutionContext
{
    /// <summary>
    /// Gets the spatial tenant coordinate slot, if assigned.
    /// </summary>
    public IVKTenantCoordinate? Tenant { get; init; }

    /// <summary>
    /// Gets the subject user coordinate slot, if assigned.
    /// </summary>
    public IVKUserCoordinate? User { get; init; }

    private VKExecutionContext(IVKTenantCoordinate? tenant, IVKUserCoordinate? user)
    {
        if (tenant is null && user is null)
        {
            throw new ArgumentException("Execution context requires at least one coordinate (Tenant or User) to be specified.");
        }

        Tenant = tenant;
        User = user;
    }

    /// <summary>
    /// Gets the active strongly-typed tenant coordinate, or null if unassigned.
    /// </summary>
    public VKTenantId? TenantId => Tenant?.TenantId;

    /// <summary>
    /// Gets the active strongly-typed user coordinate, or null if unassigned.
    /// </summary>
    public VKUserId? UserId => User?.UserId;

    /// <summary>
    /// Gets a value indicating whether a tenant coordinate has been explicitly assigned.
    /// </summary>
    public bool HasTenant => Tenant is not null;

    /// <summary>
    /// Gets a value indicating whether a user coordinate has been explicitly assigned.
    /// </summary>
    public bool HasUser => User is not null;

    /// <summary>
    /// Attempts to safely extract the active strongly-typed tenant identifier from the execution context envelope.
    /// </summary>
    /// <param name="tenantId">When this method returns, contains the tenant identifier if present; otherwise, the default value.</param>
    /// <returns><c>true</c> if a tenant coordinate is present in the context; otherwise, <c>false</c>.</returns>
    public bool TryGetTenantId([NotNullWhen(true)] out VKTenantId tenantId)
    {
        if (Tenant is { } tenant)
        {
            tenantId = tenant.TenantId;
            return true;
        }

        tenantId = default;
        return false;
    }

    /// <summary>
    /// Attempts to safely extract the active strongly-typed user identifier from the execution context envelope.
    /// </summary>
    /// <param name="userId">When this method returns, contains the user identifier if present; otherwise, the default value.</param>
    /// <returns><c>true</c> if a user coordinate is present in the context; otherwise, <c>false</c>.</returns>
    public bool TryGetUserId([NotNullWhen(true)] out VKUserId userId)
    {
        if (User is { } user)
        {
            userId = user.UserId;
            return true;
        }

        userId = default;
        return false;
    }

    /// <summary>
    /// Derives a new execution context envelope with an updated tenant coordinate while preserving existing user state.
    /// </summary>
    /// <param name="tenant">The new tenant coordinate or enriched context.</param>
    /// <returns>A new <see cref="VKExecutionContext"/> with the updated tenant slot.</returns>
    public VKExecutionContext WithTenant(IVKTenantCoordinate tenant)
    {
        VKGuard.NotNull(tenant);
        return this with { Tenant = tenant };
    }

    /// <summary>
    /// Derives a new execution context envelope with an updated user coordinate while preserving existing tenant state.
    /// </summary>
    /// <param name="user">The new user coordinate or enriched context.</param>
    /// <returns>A new <see cref="VKExecutionContext"/> with the updated user slot.</returns>
    public VKExecutionContext WithUser(IVKUserCoordinate user)
    {
        VKGuard.NotNull(user);
        return this with { User = user };
    }

    /// <summary>
    /// Creates a new execution context envelope holding only a tenant coordinate.
    /// </summary>
    /// <param name="tenant">The tenant coordinate.</param>
    public static VKExecutionContext ForTenant(IVKTenantCoordinate tenant) =>
        new(tenant: VKGuard.NotNull(tenant), user: null);

    /// <summary>
    /// Creates a new execution context envelope holding only a user coordinate.
    /// </summary>
    /// <param name="user">The user coordinate.</param>
    public static VKExecutionContext ForUser(IVKUserCoordinate user) =>
        new(tenant: null, user: VKGuard.NotNull(user));

    /// <summary>
    /// Creates a new execution context envelope holding both tenant and user coordinates.
    /// </summary>
    /// <param name="tenant">The tenant coordinate.</param>
    /// <param name="user">The user coordinate.</param>
    public static VKExecutionContext ForTenantUser(IVKTenantCoordinate tenant, IVKUserCoordinate user) =>
        new(tenant: VKGuard.NotNull(tenant), user: VKGuard.NotNull(user));
}
