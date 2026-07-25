using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Protocol for acquiring exclusive locks on session compression tasks across single or multi-node environments.
/// </summary>
public interface IVKSessionCompressionLock
{
    /// <summary>
    /// Attempts to acquire an exclusive compression lock for a specific session.
    /// </summary>
    /// <param name="sessionId">The session ID to lock.</param>
    /// <param name="leaseTime">Maximum lease duration for the lock.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A disposable handle if lock acquired successfully; otherwise failure result.</returns>
    Task<VKResult<IAsyncDisposable>> TryAcquireAsync(
        VKSessionId sessionId,
        TimeSpan leaseTime,
        CancellationToken cancellationToken = default);
}
