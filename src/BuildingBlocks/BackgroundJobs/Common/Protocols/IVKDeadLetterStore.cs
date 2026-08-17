using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Contract for dead-letter storage when jobs exhaust retries.
/// </summary>
public interface IVKDeadLetterStore
{
    Task<VKResult> StoreFailedJobAsync(string jobId, VKJobPayload payload, string reason, CancellationToken ct = default);
}
