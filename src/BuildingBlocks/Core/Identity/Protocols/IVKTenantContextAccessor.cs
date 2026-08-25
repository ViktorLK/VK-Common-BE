using System;

namespace VK.Blocks.Core;

/// <summary>
/// Accessor interface for getting, setting, and scoping ambient <see cref="IVKTenantContext"/> (Level 1).
/// Follows AP.01, AP.03, and CS.01.
/// </summary>
public interface IVKTenantContextAccessor
{
    /// <summary>
    /// Gets the current ambient tenant context, safely falling back to <see cref="VKTenantId.Default"/> when unassigned.
    /// </summary>
    IVKTenantContext Current { get; }

    /// <summary>
    /// Begins an ambient tenant-only scope that restores previous context upon disposal.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    IDisposable BeginScope(VKTenantId tenantId);

    /// <summary>
    /// Begins an ambient tenant scope with an explicit context instance that restores previous context upon disposal.
    /// </summary>
    /// <param name="context">The tenant context to apply.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    IDisposable BeginScope(IVKTenantContext context);
}
