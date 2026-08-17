namespace VK.Blocks.Caching;

/// <summary>
/// Represents a distributed lock.
/// </summary>
public interface IVKDistributedLock : IAsyncDisposable
{
    /// <summary>
    /// Gets the resource key associated with the lock.
    /// </summary>
    string ResourceKey { get; }

    /// <summary>
    /// Attempts to acquire the lock.
    /// </summary>
    Task<bool> AcquireAsync(CancellationToken ct = default);

    /// <summary>
    /// Releases the lock.
    /// </summary>
    Task ReleaseAsync(CancellationToken ct = default);
}
