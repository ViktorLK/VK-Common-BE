using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Internal;

/// <summary>
/// MultiTenancy implementation of <see cref="IVKActiveTenantProvider"/> that delegates to <see cref="IVKTenantStore"/>.
/// (AP.01)
/// </summary>
internal sealed class MultiTenancyActiveTenantProvider : IVKActiveTenantProvider
{
    private readonly IVKTenantStore _tenantStore;

    public MultiTenancyActiveTenantProvider(IVKTenantStore tenantStore)
    {
        _tenantStore = VKGuard.NotNull(tenantStore);
    }

    public async Task<VKResult<IReadOnlyList<VKTenantId>>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _tenantStore.GetActiveTenantsAsync(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure<IReadOnlyList<VKTenantId>>(result.Errors);
        }

        IReadOnlyList<VKTenantId> tenantIds = result.Value.Select(t => t.Id).ToList();
        return VKResult.Success(tenantIds);
    }
}
