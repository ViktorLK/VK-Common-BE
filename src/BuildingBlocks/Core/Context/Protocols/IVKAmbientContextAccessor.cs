using System;

namespace VK.Blocks.Core;

/// <summary>
/// Unified ambient execution flow accessor (Single Source of Truth).
/// Provides ambient coordinate querying and scoped execution across async boundaries.
/// Follows AP.01, AP.03, CS.01, and AP.06.
/// </summary>
public interface IVKAmbientContextAccessor
{
    /// <summary>
    /// Gets the current ambient tenant coordinate (Level 1), or null if unassigned in the active async flow.
    /// </summary>
    IVKTenantCoordinate? CurrentTenantCoordinate { get; }

    /// <summary>
    /// Gets the current ambient user coordinate (Level 2), or null if unassigned in the active async flow.
    /// </summary>
    IVKUserCoordinate? CurrentUserCoordinate { get; }

    /// <summary>
    /// Gets the current composite execution context envelope, or null if unassigned.
    /// </summary>
    VKExecutionContext? CurrentContext { get; }

    /// <summary>
    /// Begins an ambient tenant-only scope that restores the previous context upon disposal.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    IDisposable BeginScope(VKTenantId tenantId);

    /// <summary>
    /// Begins an ambient user-only scope while preserving existing tenant state upon disposal.
    /// </summary>
    /// <param name="userId">The active user identifier.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    IDisposable BeginScope(VKUserId userId);

    /// <summary>
    /// Begins an ambient identity scope with strongly-typed tenant and user coordinates.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="userId">The active user identifier.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    IDisposable BeginScope(VKTenantId tenantId, VKUserId userId);

    /// <summary>
    /// Begins an ambient scope with an explicit tenant coordinate or enriched context instance.
    /// </summary>
    /// <param name="coordinate">The tenant coordinate to push into the ambient flow.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    IDisposable BeginScope(IVKTenantCoordinate coordinate);

    /// <summary>
    /// Begins an ambient scope with an explicit user coordinate or enriched context instance.
    /// </summary>
    /// <param name="coordinate">The user coordinate to push into the ambient flow.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    IDisposable BeginScope(IVKUserCoordinate coordinate);

    /// <summary>
    /// Begins an ambient scope with explicit tenant and user coordinates or enriched context instances.
    /// </summary>
    /// <param name="tenant">The tenant coordinate to push into the ambient flow.</param>
    /// <param name="user">The user coordinate to push into the ambient flow.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    IDisposable BeginScope(IVKTenantCoordinate tenant, IVKUserCoordinate user);

    /// <summary>
    /// Begins an ambient scope with a full composite execution context envelope.
    /// </summary>
    /// <param name="context">The composite execution context to push into the ambient flow.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    IDisposable BeginScope(VKExecutionContext context);
}
