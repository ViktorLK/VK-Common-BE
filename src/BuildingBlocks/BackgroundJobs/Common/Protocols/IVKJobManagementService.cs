using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Interface for manual job triggering, replay, and cancellation.
/// </summary>
public interface IVKJobManagementService
{
    Task<VKResult> ReplayJobAsync(string jobId, CancellationToken ct = default);
    Task<VKResult> CancelJobAsync(string jobId, CancellationToken ct = default);
}
