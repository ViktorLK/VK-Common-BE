using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Context.Internal;

/// <summary>
/// Scoped dynamic accessor of <see cref="IVKTenantContext"/> backed by <see cref="VKAmbientIdentityContext"/>.
/// Follows AP.01, AP.03, AP.06.
/// </summary>
internal sealed class TenantContextAccessor : IVKTenantContext
{
    private static IVKTenantContext? ActiveContext => VKAmbientExecutionContext.Current?.Tenant as IVKTenantContext;

    /// <inheritdoc />
    public bool IsResolved => ActiveContext is not null;

    /// <inheritdoc />
    public VKTenantId TenantId => VKAmbientExecutionContext.Current?.TenantId ?? throw new InvalidOperationException("Multi-tenant execution requires a resolved active tenant. No tenant is associated with the current context.");

    /// <inheritdoc />
    public string TenantName => ActiveContext?.TenantName ?? string.Empty;

    /// <inheritdoc />
    public string? Domain => ActiveContext?.Domain;

    /// <inheritdoc />
    public bool IsActive => ActiveContext?.IsActive ?? false;

    /// <inheritdoc />
    public VKSensitiveString? ConnectionString => ActiveContext?.ConnectionString;

    /// <inheritdoc />
    public string? Schema => ActiveContext?.Schema;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Metadata => ActiveContext?.Metadata ?? FrozenDictionary<string, string>.Empty;
}
