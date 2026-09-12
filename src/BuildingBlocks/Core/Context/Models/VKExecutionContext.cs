using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Core;

/// <summary>
/// Unified immutable composite execution context envelope residing in the ambient async flow (SSoT).
/// Combines spatial (<see cref="IVKTenantCoordinate"/>), subject (<see cref="IVKUserCoordinate"/>), and extensible domain feature slots.
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

    /// <summary>
    /// Gets the immutable collection of domain or feature coordinate slots attached to this execution context.
    /// </summary>
    public ImmutableDictionary<Type, object> Slots { get; init; } = ImmutableDictionary<Type, object>.Empty;

    private VKExecutionContext(
        IVKTenantCoordinate? tenant,
        IVKUserCoordinate? user,
        ImmutableDictionary<Type, object>? slots = null)
    {
        var resolvedSlots = slots ?? ImmutableDictionary<Type, object>.Empty;
        if (tenant is null && user is null && resolvedSlots.IsEmpty)
        {
            throw new ArgumentException("Execution context requires at least one coordinate (Tenant, User, or Slot) to be specified.");
        }

        Tenant = tenant;
        User = user;
        Slots = resolvedSlots;
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
        // [AP.01]
        VKGuard.NotNull(user);
        return this with { User = user };
    }

    /// <summary>
    /// Gets a value indicating whether a coordinate or feature slot of type <typeparamref name="TSlot"/> has been explicitly assigned.
    /// </summary>
    /// <typeparam name="TSlot">The type of slot object or interface.</typeparam>
    /// <returns><c>true</c> if the slot is present in the context; otherwise, <c>false</c>.</returns>
    public bool HasSlot<TSlot>() where TSlot : class => Slots.ContainsKey(typeof(TSlot));

    /// <summary>
    /// Gets the strongly-typed coordinate or feature slot from the execution context envelope, or null if unassigned.
    /// </summary>
    /// <typeparam name="TSlot">The type of slot object or interface.</typeparam>
    /// <returns>The assigned slot instance if present; otherwise, <c>null</c>.</returns>
    public TSlot? GetSlot<TSlot>() where TSlot : class
    {
        return Slots.TryGetValue(typeof(TSlot), out var value) ? value as TSlot : null;
    }

    /// <summary>
    /// Attempts to safely extract a strongly-typed coordinate or feature slot from the execution context envelope.
    /// </summary>
    /// <typeparam name="TSlot">The type of slot object or interface.</typeparam>
    /// <param name="slot">When this method returns, contains the slot instance if present; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the slot is present in the context; otherwise, <c>false</c>.</returns>
    public bool TryGetSlot<TSlot>([NotNullWhen(true)] out TSlot? slot) where TSlot : class
    {
        if (Slots.TryGetValue(typeof(TSlot), out var value) && value is TSlot typed)
        {
            slot = typed;
            return true;
        }

        slot = null;
        return false;
    }

    /// <summary>
    /// Derives a new execution context envelope with an added or updated slot while preserving existing tenant, user, and other slot states.
    /// </summary>
    /// <typeparam name="TSlot">The type of slot object or interface.</typeparam>
    /// <param name="slot">The slot instance to attach.</param>
    /// <returns>A new <see cref="VKExecutionContext"/> with the updated slot.</returns>
    public VKExecutionContext WithSlot<TSlot>(TSlot slot) where TSlot : class
    {
        // [AP.01]
        VKGuard.NotNull(slot);
        return this with { Slots = Slots.SetItem(typeof(TSlot), slot) };
    }

    /// <summary>
    /// Derives a new execution context envelope with the specified slot removed while preserving other slots, tenant, and user state.
    /// </summary>
    /// <typeparam name="TSlot">The type of slot object or interface.</typeparam>
    /// <returns>A new <see cref="VKExecutionContext"/> with the slot removed.</returns>
    public VKExecutionContext WithoutSlot<TSlot>() where TSlot : class
    {
        return this with { Slots = Slots.Remove(typeof(TSlot)) };
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

    /// <summary>
    /// Creates a new execution context envelope holding only a custom coordinate or feature slot.
    /// </summary>
    /// <typeparam name="TSlot">The type of slot object or interface.</typeparam>
    /// <param name="slot">The slot instance.</param>
    /// <returns>A new <see cref="VKExecutionContext"/> initialized with the given slot.</returns>
    public static VKExecutionContext ForSlot<TSlot>(TSlot slot) where TSlot : class =>
        new(tenant: null, user: null, slots: ImmutableDictionary<Type, object>.Empty.Add(typeof(TSlot), VKGuard.NotNull(slot)));
}

