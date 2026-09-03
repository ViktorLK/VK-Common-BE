using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Tenants.Internal;

/// <summary>
/// Implementation of <see cref="IVKActiveTenantProvider"/> that queries <see cref="IVKTenantStore"/>
/// to provide active tenant IDs for background jobs and system workers.
/// </summary>
internal sealed class MultiTenancyActiveTenantProvider(IVKTenantStore tenantStore) : IVKActiveTenantProvider
{
    private readonly IVKTenantStore _tenantStore = VKGuard.NotNull(tenantStore);

    /// <inheritdoc />
    public async Task<VKResult<IReadOnlyList<VKTenantId>>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _tenantStore.GetActiveTenantsAsync(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return VKResult.Failure<IReadOnlyList<VKTenantId>>(result.FirstError);
        }

        IReadOnlyList<VKTenantId> ids = result.Value.Select(t => t.Id).ToList();
        return VKResult.Success(ids);
    }
}
