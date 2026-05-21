using System;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the contract for managing and freezing tenant-specific execution sandboxes.
/// Follows AP.03.
/// </summary>
public interface IVKTenantContextAccessor
{
    /// <summary>
    /// Freezes the current execution to the specified tenant context.
    /// </summary>
    /// <param name="tenantId">The active tenant identifier to freeze in scope.</param>
    /// <returns>An <see cref="IDisposable"/> that releases the frozen sandbox when disposed.</returns>
    IDisposable Freeze(string tenantId);

    /// <summary>
    /// Gets the currently active/frozen tenant identifier in the active execution context.
    /// </summary>
    /// <returns>The active tenant identifier, or null if none is bound.</returns>
    string? GetActiveTenantId();
}
