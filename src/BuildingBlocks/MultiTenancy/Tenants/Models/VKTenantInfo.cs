using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Lightweight, immutable runtime tenant descriptor used by multi-tenancy resolution pipelines.
/// Follows AP.01, AP.03, CS.01.
/// </summary>
public sealed record VKTenantInfo
{
    /// <summary>
    /// Gets the unique tenant identifier.
    /// </summary>
    public required VKTenantId Id { get; init; }

    /// <summary>
    /// Gets the display name of the tenant.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the optional primary domain associated with the tenant.
    /// </summary>
    public string? Domain { get; init; }

    /// <summary>
    /// Gets a value indicating whether the tenant is currently active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Gets the optional connection string specific to this tenant's data store.
    /// </summary>
    public VKSensitiveString? ConnectionString { get; init; }

    /// <summary>
    /// Gets the optional database schema specific to this tenant.
    /// </summary>
    public string? Schema { get; init; }

    /// <summary>
    /// Gets the immutable tenant metadata dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = FrozenDictionary<string, string>.Empty;

    /// <summary>
    /// Static factory entry point to create a new instance of <see cref="VKTenantInfo"/>.
    /// </summary>
    /// <param name="id">The unique tenant identifier.</param>
    /// <param name="name">The display name of the tenant.</param>
    /// <param name="domain">The optional primary domain.</param>
    /// <param name="isActive">Whether the tenant is currently active.</param>
    /// <param name="connectionString">The optional database connection string.</param>
    /// <param name="schema">The optional database schema.</param>
    /// <param name="metadata">The optional metadata dictionary.</param>
    /// <returns>A new <see cref="VKTenantInfo"/> instance.</returns>
    public static VKTenantInfo Create(
        VKTenantId id,
        string name,
        string? domain = null,
        bool isActive = true,
        VKSensitiveString? connectionString = null,
        string? schema = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        VKGuard.NotDefault(id);
        VKGuard.NotNullOrWhiteSpace(name);

        return new VKTenantInfo
        {
            Id = id,
            Name = name,
            Domain = string.IsNullOrWhiteSpace(domain) ? null : domain.Trim().ToLowerInvariant(),
            IsActive = isActive,
            ConnectionString = connectionString,
            Schema = string.IsNullOrWhiteSpace(schema) ? null : schema.Trim(),
            Metadata = metadata ?? FrozenDictionary<string, string>.Empty
        };
    }
}
