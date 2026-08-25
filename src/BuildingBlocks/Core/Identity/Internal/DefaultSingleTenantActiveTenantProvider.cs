using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Default single-tenant fallback implementation for <see cref="IVKActiveTenantProvider"/>.
/// Used when MultiTenancy block is not registered or in single-tenant environments. (AP.01)
/// </summary>
internal sealed class DefaultSingleTenantActiveTenantProvider : IVKActiveTenantProvider
{
    private static readonly IReadOnlyList<VKTenantId> DefaultTenantList = [VKTenantId.Default];

    public Task<VKResult<IReadOnlyList<VKTenantId>>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(VKResult.Success(DefaultTenantList));
    }
}
