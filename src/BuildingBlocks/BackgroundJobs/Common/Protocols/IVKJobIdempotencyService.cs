using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Contract for job idempotency validation and deduplication.
/// </summary>
public interface IVKJobIdempotencyService
{
    Task<VKResult<bool>> IsProcessedAsync(VKIdempotencyKey key, CancellationToken ct = default);
    Task<VKResult> MarkProcessedAsync(VKIdempotencyKey key, string jobId, CancellationToken ct = default);
}
