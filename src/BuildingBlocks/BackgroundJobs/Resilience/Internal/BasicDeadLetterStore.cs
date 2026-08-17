using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Resilience.Internal;

internal sealed class BasicDeadLetterStore : IVKDeadLetterStore
{
    private readonly ConcurrentDictionary<string, (VKJobPayload Payload, string Reason)> _deadLetterJobs = new();

    public Task<VKResult> StoreFailedJobAsync(string jobId, VKJobPayload payload, string reason, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);
        VKGuard.NotNull(payload);
        _deadLetterJobs[jobId] = (payload, reason);
        return Task.FromResult(VKResult.Success());
    }
}
