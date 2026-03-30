namespace VK.Blocks.Caching.Abstractions;

/// <summary>
/// Factory for creating distributed locks.
/// </summary>
public interface IDistributedLockProvider
{
    /// <summary>
    /// Creates a lock for the specified resource.
    /// </summary>
    IDistributedLock CreateLock(string resourceKey, TimeSpan? expiry = null);
}
