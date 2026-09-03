using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Context.Internal;

/// <summary>
/// Immutable snapshot execution context holding resolved tenant properties.
/// Implements both <see cref="IVKTenantContext"/> and <see cref="IVKTenantCoordinate"/> for polymorphic dispatching in <see cref="VKAmbientExecutionContext"/>.
/// Follows AP.01, AP.03, CS.01.
/// </summary>
internal sealed record DefaultTenantContext(
    VKTenantId TenantId,
    string TenantName,
    string? Domain = null,
    bool IsActive = true,
    string? Schema = null,
    VKSensitiveString? ConnectionString = null,
    IReadOnlyDictionary<string, string>? Metadata = null) : IVKTenantContext
{
    /// <inheritdoc />
    public bool IsResolved => true;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = Metadata ?? FrozenDictionary<string, string>.Empty;

    /// <summary>
    /// Creates an immutable context snapshot from a runtime <see cref="VKTenantInfo"/> descriptor.
    /// </summary>
    public static DefaultTenantContext FromTenantInfo(VKTenantInfo tenantInfo)
    {
        VKGuard.NotNull(tenantInfo);
        return new DefaultTenantContext(
            tenantInfo.Id,
            tenantInfo.Name,
            tenantInfo.Domain,
            tenantInfo.IsActive,
            tenantInfo.Schema,
            tenantInfo.ConnectionString,
            tenantInfo.Metadata);
    }
}
