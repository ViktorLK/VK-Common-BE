using System.Collections.Generic;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Defines the base contract for tenant information within the VK.Blocks ecosystem.
/// </summary>
public interface IVKTenantInfo
{
    /// <summary>
    /// Gets the unique identifier for the tenant.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the human-readable name of the tenant.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the primary domain associated with the tenant.
    /// </summary>
    string? Domain { get; }

    /// <summary>
    /// Gets a value indicating whether the tenant is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets the optional database schema specific to this tenant.
    /// </summary>
    string? Schema { get; }

    /// <summary>
    /// Gets the optional connection string specific to this tenant's data store.
    /// </summary>
    string? ConnectionString { get; }

    /// <summary>
    /// Gets additional metadata associated with the tenant (e.g. configuration, feature flags).
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
