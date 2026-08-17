using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Jobs.Internal;

internal sealed class BasicJobStore : IVKJobStore
{
    private readonly ConcurrentDictionary<string, VKJobPayload> _jobs = new();

    public Task<VKResult> SaveJobAsync(string jobId, VKJobPayload payload, string queue, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);
        VKGuard.NotNull(payload);

        _jobs[jobId] = payload;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<VKJobPayload?>> GetJobAsync(string jobId, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);
        _jobs.TryGetValue(jobId, out var payload);
        return Task.FromResult(VKResult.Success(payload));
    }

    public Task<VKResult> RemoveJobAsync(string jobId, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);
        _jobs.TryRemove(jobId, out _);
        return Task.FromResult(VKResult.Success());
    }
}
