namespace VK.Blocks.Caching.Abstractions;

/// <summary>
/// Defines a contract for a distributed lock.
/// </summary>
public interface IDistributedLock : IAsyncDisposable
{
    /// <summary>
    /// Gets a value indicating whether the lock is held.
    /// </summary>
    bool IsAcquired { get; }

    /// <summary>
    /// Attempts to acquire the lock.
    /// </summary>
    Task<bool> AcquireAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases the lock.
    /// </summary>
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}
