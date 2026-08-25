using System;

namespace VK.Blocks.Core;

/// <summary>
/// Accessor interface for getting, setting, and scoping ambient <see cref="IVKIdentityContext"/> across asynchronous operations.
/// Follows AP.01, AP.03, and CS.01.
/// </summary>
public interface IVKIdentityContextAccessor
{
    /// <summary>
    /// Gets the current ambient identity context, falling back to system/anonymous default when not set.
    /// </summary>
    IVKIdentityContext Current { get; }

    /// <summary>
    /// Begins an ambient tenant-only scope (Level 1) that restores previous context upon disposal.
    /// User identity inside this scope safely falls back to <see cref="VKUserId.Anonymous"/>.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    IDisposable BeginTenantScope(VKTenantId tenantId);

    /// <summary>
    /// Begins an ambient identity scope with strongly-typed coordinates that restores previous identity upon disposal.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="userId">The active user identifier.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous identity upon disposal.</returns>
    IDisposable BeginScope(VKTenantId tenantId, VKUserId userId);

    /// <summary>
    /// Begins an ambient identity scope with an explicit context instance that restores previous identity upon disposal.
    /// </summary>
    /// <param name="context">The identity context to apply.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous identity upon disposal.</returns>
    IDisposable BeginScope(IVKIdentityContext context);
}
