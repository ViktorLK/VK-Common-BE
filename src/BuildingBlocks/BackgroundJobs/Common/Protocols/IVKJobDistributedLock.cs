using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Distributed lock contract for multi-instance recurring job execution.
/// </summary>
public interface IVKJobDistributedLock
{
    Task<VKResult<IDisposable?>> TryAcquireLockAsync(string resourceKey, TimeSpan timeout, CancellationToken ct = default);
}
