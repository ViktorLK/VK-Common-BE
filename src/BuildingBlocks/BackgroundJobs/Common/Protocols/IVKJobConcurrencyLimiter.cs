using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Interface for per-job-type concurrency throttling.
/// </summary>
public interface IVKJobConcurrencyLimiter
{
    Task<VKResult<IDisposable>> AcquireLockAsync(string jobType, CancellationToken ct = default);
}
