using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Management.Internal;

internal sealed class DefaultJobManagementService : IVKJobManagementService
{
    private readonly IVKJobStateStore _stateStore;

    public DefaultJobManagementService(IVKJobStateStore stateStore)
    {
        _stateStore = VKGuard.NotNull(stateStore);
    }

    public async Task<VKResult> ReplayJobAsync(string jobId, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);
        await _stateStore.SetStateAsync(jobId, VKJobState.Enqueued, "Replay", ct).ConfigureAwait(false);
        return VKResult.Success();
    }

    public async Task<VKResult> CancelJobAsync(string jobId, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);
        await _stateStore.SetStateAsync(jobId, VKJobState.Deleted, "Cancelled", ct).ConfigureAwait(false);
        return VKResult.Success();
    }
}
