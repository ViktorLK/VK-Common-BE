using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Interface for tracking and querying job lifecycle states.
/// </summary>
public interface IVKJobStateStore
{
    Task<VKResult> SetStateAsync(string jobId, VKJobState state, string? reason = null, CancellationToken ct = default);
    Task<VKResult<VKJobState?>> GetStateAsync(string jobId, CancellationToken ct = default);
}
