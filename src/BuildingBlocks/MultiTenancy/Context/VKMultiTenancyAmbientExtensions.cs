using System;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy.Context.Internal;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Extension methods on <see cref="IVKAmbientContextAccessor"/> for rich tenant scoping.
/// Follows AP.01, AP.03.
/// </summary>
public static class VKMultiTenancyAmbientExtensions
{
    /// <summary>
    /// Begins an ambient tenant scope using an immutable snapshot extracted from the <see cref="VKTenantInfo"/> descriptor.
    /// </summary>
    /// <param name="accessor">The ambient context accessor.</param>
    /// <param name="tenantInfo">The tenant descriptor.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    public static IDisposable BeginScope(this IVKAmbientContextAccessor accessor, VKTenantInfo tenantInfo)
    {
        VKGuard.NotNull(accessor);
        VKGuard.NotNull(tenantInfo);
        return accessor.BeginScope(DefaultTenantContext.FromTenantInfo(tenantInfo));
    }

    /// <summary>
    /// Begins an ambient tenant scope using the specified tenant identifier and optional name.
    /// </summary>
    /// <param name="accessor">The ambient context accessor.</param>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="tenantName">The optional display name of the tenant.</param>
    /// <returns>An <see cref="IDisposable"/> token that restores the previous context upon disposal.</returns>
    public static IDisposable BeginScope(this IVKAmbientContextAccessor accessor, VKTenantId tenantId, string? tenantName = null)
    {
        VKGuard.NotNull(accessor);
        VKGuard.NotDefault(tenantId);
        return accessor.BeginScope(new DefaultTenantContext(tenantId, tenantName ?? tenantId.ToString()));
    }
}
