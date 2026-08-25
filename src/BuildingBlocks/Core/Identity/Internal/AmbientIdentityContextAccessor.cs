using System;

namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Thread-safe implementation of <see cref="IVKIdentityContextAccessor"/> backed by the unified <see cref="AsyncLocalExecutionContextStore"/>.
/// Supports Level 1 (Tenant only) and Level 2 (Tenant + User) scopes.
/// Follows AP.01 and AP.03.
/// </summary>
internal sealed class AmbientIdentityContextAccessor : IVKIdentityContextAccessor
{
    /// <inheritdoc />
    public IVKIdentityContext Current
    {
        get
        {
            var raw = AsyncLocalExecutionContext.Current;
            if (raw is IVKIdentityContext idCtx)
            {
                return idCtx;
            }

            // Level 1 Tenant-only context: project to Identity with Anonymous user
            return new DefaultIdentityContext(raw.TenantId, VKUserId.Anonymous);
        }
    }

    /// <inheritdoc />
    public IDisposable BeginTenantScope(VKTenantId tenantId)
    {
        VKGuard.NotDefault(tenantId);
        return AsyncLocalExecutionContext.BeginScope(new DefaultTenantContext(tenantId));
    }

    /// <inheritdoc />
    public IDisposable BeginScope(VKTenantId tenantId, VKUserId userId)
    {
        VKGuard.NotDefault(tenantId);
        VKGuard.NotDefault(userId);
        return BeginScope(new DefaultIdentityContext(tenantId, userId));
    }

    /// <inheritdoc />
    public IDisposable BeginScope(IVKIdentityContext context)
    {
        VKGuard.NotNull(context);
        return AsyncLocalExecutionContext.BeginScope(context);
    }
}
