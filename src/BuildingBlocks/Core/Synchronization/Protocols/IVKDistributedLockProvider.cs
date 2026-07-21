using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Defines a contract for acquiring distributed/process-level locks.
/// </summary>
public interface IVKDistributedLockProvider
{
    /// <summary>
    /// Attempts to acquire an exclusive lock for the specified key.
    /// </summary>
    /// <param name="lockKey">The unique lock identifier.</param>
    /// <param name="expiry">The maximum duration before the lock expires automatically.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A handle to the lock if acquired, or null if lock acquisition failed.</returns>
    ValueTask<IVKDistributedLockHandle?> TryAcquireLockAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an acquired lock handle. Must be disposed to release the lock.
/// </summary>
public interface IVKDistributedLockHandle : IAsyncDisposable
{
    /// <summary>
    /// Gets a value indicating whether the lock is currently held.
    /// </summary>
    bool IsAcquired { get; }
}
