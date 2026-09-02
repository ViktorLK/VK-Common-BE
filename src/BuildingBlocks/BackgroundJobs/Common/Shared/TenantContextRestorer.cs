using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Shared;

internal sealed class TenantContextRestorer
{
    private readonly IVKTenantCoordinate? _tenantCoordinate;

    public TenantContextRestorer(IVKTenantCoordinate? tenantCoordinate = null)
    {
        _tenantCoordinate = tenantCoordinate;
    }

    public void RestoreContext(string? tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return;
        }
    }
}
