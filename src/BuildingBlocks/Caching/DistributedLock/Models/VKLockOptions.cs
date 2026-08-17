namespace VK.Blocks.Caching;

/// <summary>
/// Configuration for acquiring a distributed lock.
/// </summary>
public sealed record VKLockOptions
{
    /// <summary>
    /// Gets the duration the lock remains valid.
    /// </summary>
    public TimeSpan LockTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets the duration to wait before giving up on acquiring the lock.
    /// </summary>
    public TimeSpan AcquireTimeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets the interval to wait between lock acquisition attempts.
    /// </summary>
    public TimeSpan RetryInterval { get; init; } = TimeSpan.FromMilliseconds(50);
}
