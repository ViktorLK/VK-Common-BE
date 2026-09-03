using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Immutable, read-only multi-tenant execution context snapshot.
/// Extends the core coordinate contract <see cref="IVKTenantCoordinate"/>.
/// Follows AP.01, AP.03, AP.06, and CS.01.
/// </summary>
public interface IVKTenantContext : IVKTenantCoordinate
{
    /// <summary>
    /// Gets a value indicating whether a tenant has been successfully resolved for the active execution context.
    /// </summary>
    bool IsResolved { get; }

    /// <summary>
    /// Gets the display name of the current tenant, or empty if unresolved.
    /// </summary>
    string TenantName { get; }

    /// <summary>
    /// Gets the primary domain associated with the current tenant, or null if unresolved.
    /// </summary>
    string? Domain { get; }

    /// <summary>
    /// Gets a value indicating whether the current tenant is active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets the optional connection string specific to this tenant's data store.
    /// </summary>
    VKSensitiveString? ConnectionString { get; }

    /// <summary>
    /// Gets the optional database schema specific to this tenant.
    /// </summary>
    string? Schema { get; }

    /// <summary>
    /// Gets the immutable tenant metadata dictionary.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
