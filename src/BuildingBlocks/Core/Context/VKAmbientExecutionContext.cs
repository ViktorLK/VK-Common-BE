using System;
using System.Threading;
using VK.Blocks.Core.Context.Internal;

namespace VK.Blocks.Core;

/// <summary>
/// Thread-safe ambient execution context managing the composite <see cref="VKExecutionContext"/> envelope (SSoT).
/// Combines spatial (<see cref="IVKTenantCoordinate"/>) and subject (<see cref="IVKUserCoordinate"/>) slots across async operations.
/// Follows AP.01, AP.03, AP.06, and OR.02.
/// </summary>
public static class VKAmbientExecutionContext
{
    private static readonly AsyncLocal<VKExecutionContext?> _current = new();

    /// <summary>
    /// Gets the current ambient execution context envelope, or null if no context has been pushed into the async flow.
    /// </summary>
    public static VKExecutionContext? Current => _current.Value;

    /// <summary>
    /// Gets a value indicating whether an explicit ambient execution context has been assigned to the active async flow.
    /// </summary>
    public static bool HasContext => _current.Value is not null;

    /// <summary>
    /// Pushes an active execution context envelope into the ambient flow and returns an <see cref="IDisposable"/> token that reverts to the prior context on disposal.
    /// </summary>
    /// <param name="context">The composite execution context envelope to apply.</param>
    /// <returns>A scope token that restores the prior ambient context upon disposal.</returns>
    public static IDisposable BeginScope(VKExecutionContext context)
    {
        VKGuard.NotNull(context);
        var prior = _current.Value;
        _current.Value = context;
        return new ScopeToken(prior);
    }

    /// <summary>
    /// Pushes an active tenant coordinate or enriched context into the ambient flow.
    /// If an existing user coordinate is present in the flow, it is preserved.
    /// </summary>
    /// <param name="tenant">The tenant coordinate or enriched context to apply.</param>
    /// <returns>A scope token that restores the prior ambient context upon disposal.</returns>
    public static IDisposable BeginScope(IVKTenantCoordinate tenant)
    {
        VKGuard.NotNull(tenant);
        var active = _current.Value;
        var next = active is not null ? active.WithTenant(tenant) : VKExecutionContext.ForTenant(tenant);
        return BeginScope(next);
    }

    /// <summary>
    /// Pushes an active user coordinate or enriched context into the ambient flow.
    /// If an existing tenant coordinate is present in the flow, it is preserved.
    /// </summary>
    /// <param name="user">The user coordinate or enriched context to apply.</param>
    /// <returns>A scope token that restores the prior ambient context upon disposal.</returns>
    public static IDisposable BeginScope(IVKUserCoordinate user)
    {
        VKGuard.NotNull(user);
        var active = _current.Value;
        var next = active is not null ? active.WithUser(user) : VKExecutionContext.ForUser(user);
        return BeginScope(next);
    }

    /// <summary>
    /// Pushes active tenant and user coordinates or enriched contexts into the ambient flow.
    /// </summary>
    /// <param name="tenant">The tenant coordinate or enriched context.</param>
    /// <param name="user">The user coordinate or enriched context.</param>
    /// <returns>A scope token that restores the prior ambient context upon disposal.</returns>
    public static IDisposable BeginScope(IVKTenantCoordinate tenant, IVKUserCoordinate user)
    {
        VKGuard.NotNull(tenant);
        VKGuard.NotNull(user);
        return BeginScope(VKExecutionContext.ForTenantUser(tenant, user));
    }

    /// <summary>
    /// Pushes a strongly-typed tenant identifier coordinate into the ambient flow.
    /// </summary>
    public static IDisposable BeginScope(VKTenantId tenantId)
    {
        VKGuard.NotDefault(tenantId);
        return BeginScope(new DefaultTenantCoordinate(tenantId));
    }

    /// <summary>
    /// Pushes a strongly-typed user identifier coordinate into the ambient flow while preserving existing tenant state.
    /// </summary>
    public static IDisposable BeginScope(VKUserId userId)
    {
        VKGuard.NotDefault(userId);
        return BeginScope(new DefaultUserCoordinate(userId));
    }

    /// <summary>
    /// Pushes strongly-typed tenant and user identifier coordinates into the ambient flow.
    /// </summary>
    public static IDisposable BeginScope(VKTenantId tenantId, VKUserId userId)
    {
        VKGuard.NotDefault(tenantId);
        VKGuard.NotDefault(userId);
        return BeginScope(VKExecutionContext.ForTenantUser(
            new DefaultTenantCoordinate(tenantId),
            new DefaultUserCoordinate(userId)));
    }

    private sealed class ScopeToken(VKExecutionContext? prior) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _current.Value = prior;
            }
        }
    }
}
