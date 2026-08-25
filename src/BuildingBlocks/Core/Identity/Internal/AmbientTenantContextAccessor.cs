using System;

namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Thread-safe implementation of <see cref="IVKTenantContextAccessor"/> backed by the unified <see cref="AsyncLocalExecutionContext"/>.
/// Follows AP.01 and AP.03.
/// </summary>
internal sealed class AmbientTenantContextAccessor : IVKTenantContextAccessor
{
    /// <inheritdoc />
    public IVKTenantContext Current => AsyncLocalExecutionContext.Current;

    /// <inheritdoc />
    public IDisposable BeginScope(VKTenantId tenantId)
    {
        VKGuard.NotDefault(tenantId);
        return AsyncLocalExecutionContext.BeginScope(new DefaultTenantContext(tenantId));
    }

    /// <inheritdoc />
    public IDisposable BeginScope(IVKTenantContext context)
    {
        VKGuard.NotNull(context);
        return AsyncLocalExecutionContext.BeginScope(context);
    }
}
