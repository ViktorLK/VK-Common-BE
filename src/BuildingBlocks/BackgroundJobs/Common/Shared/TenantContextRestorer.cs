using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Shared;

internal sealed class TenantContextRestorer
{
    private readonly IVKSecurityContext? _userContext;

    public TenantContextRestorer(IVKSecurityContext? userContext = null)
    {
        _userContext = userContext;
    }

    public void RestoreContext(string? tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return;
        }
    }
}
