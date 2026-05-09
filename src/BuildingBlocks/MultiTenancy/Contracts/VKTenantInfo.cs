using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Represents the standard implementation of tenant information.
/// </summary>
public sealed record VKTenantInfo : IVKTenantInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKTenantInfo"/> record.
    /// </summary>
    [SetsRequiredMembers]
    public VKTenantInfo(
        string id,
        string name,
        string? domain = null,
        bool isActive = true,
        string? connectionString = null,
        string? schema = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        Id = VKGuard.NotNullOrWhiteSpace(id);
        Name = VKGuard.NotNullOrWhiteSpace(name);
        Domain = domain;
        IsActive = isActive;
        ConnectionString = connectionString;
        Schema = schema;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets the unique identifier of the tenant.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the display name of the tenant.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the primary domain associated with the tenant.
    /// </summary>
    public string? Domain { get; init; }

    /// <summary>
    /// Gets a value indicating whether the tenant is currently active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Gets the optional connection string specific to this tenant's data store.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Gets the optional database schema specific to this tenant.
    /// </summary>
    public string? Schema { get; init; }

    /// <summary>
    /// Gets optional metadata associated with the tenant (e.g., tier, feature flags).
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; }
}
