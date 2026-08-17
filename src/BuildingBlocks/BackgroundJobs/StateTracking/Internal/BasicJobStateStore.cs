using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.StateTracking.Internal;

internal sealed class BasicJobStateStore : IVKJobStateStore
{
    private readonly ConcurrentDictionary<string, VKJobState> _states = new();

    public Task<VKResult> SetStateAsync(string jobId, VKJobState state, string? reason = null, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);
        _states[jobId] = state;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<VKJobState?>> GetStateAsync(string jobId, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);
        _states.TryGetValue(jobId, out var state);
        return Task.FromResult(VKResult.Success<VKJobState?>(state));
    }
}
