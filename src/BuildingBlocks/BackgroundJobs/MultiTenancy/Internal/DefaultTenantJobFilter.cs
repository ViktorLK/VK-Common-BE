using VK.Blocks.BackgroundJobs.Shared;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.MultiTenancy.Internal;

internal sealed class DefaultTenantJobFilter
{
    private readonly TenantContextRestorer _restorer;

    public DefaultTenantJobFilter(TenantContextRestorer restorer)
    {
        _restorer = VKGuard.NotNull(restorer);
    }

    public void OnJobExecuting(VKJobContext context)
    {
        VKGuard.NotNull(context);
        _restorer.RestoreContext(context.TenantId);
    }
}
