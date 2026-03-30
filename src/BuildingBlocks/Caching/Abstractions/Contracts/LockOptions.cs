namespace VK.Blocks.Caching.Abstractions.Contracts;

/// <summary>
/// Configuration for distributed lock operations.
/// </summary>
public sealed record LockOptions
{
    /// <summary>
    /// Gets the maximum time to wait for the lock.
    /// </summary>
    public TimeSpan LockTimeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets the maximum time to wait for acquiring the lock (blocking).
    /// </summary>
    public TimeSpan AcquireTimeout { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the interval between retries when acquiring the lock.
    /// </summary>
    public TimeSpan RetryInterval { get; init; } = TimeSpan.FromMilliseconds(100);
}
