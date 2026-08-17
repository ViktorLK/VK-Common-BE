using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Abstract storage provider for jobs and queue partitions.
/// </summary>
public interface IVKJobStore
{
    Task<VKResult> SaveJobAsync(string jobId, VKJobPayload payload, string queue, CancellationToken ct = default);
    Task<VKResult<VKJobPayload?>> GetJobAsync(string jobId, CancellationToken ct = default);
    Task<VKResult> RemoveJobAsync(string jobId, CancellationToken ct = default);
}
