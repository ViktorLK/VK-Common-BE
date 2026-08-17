using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Options for the DistributedLock feature slice.
/// </summary>

public sealed partial record VKDistributedLockOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the duration the lock remains valid.
    /// </summary>
    public TimeSpan DefaultLockTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets the duration to wait before giving up on acquiring the lock.
    /// </summary>
    public TimeSpan DefaultAcquireTimeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets the interval to wait between lock acquisition attempts.
    /// </summary>
    public TimeSpan DefaultRetryInterval { get; init; } = TimeSpan.FromMilliseconds(50);
}
