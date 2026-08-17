using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Idempotency.Internal;

internal sealed class DefaultJobIdempotencyService : IVKJobIdempotencyService
{
    private readonly ConcurrentDictionary<string, string> _processedKeys = new();

    public Task<VKResult<bool>> IsProcessedAsync(VKIdempotencyKey key, CancellationToken ct = default)
    {
        VKGuard.NotNull(key);
        var isProcessed = _processedKeys.ContainsKey(key.Value);
        return Task.FromResult(VKResult.Success(isProcessed));
    }

    public Task<VKResult> MarkProcessedAsync(VKIdempotencyKey key, string jobId, CancellationToken ct = default)
    {
        VKGuard.NotNull(key);
        VKGuard.NotNullOrWhiteSpace(jobId);
        _processedKeys[key.Value] = jobId;
        return Task.FromResult(VKResult.Success());
    }
}
