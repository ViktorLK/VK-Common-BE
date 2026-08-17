namespace VK.Blocks.Caching;

/// <summary>
/// Provider for acquiring distributed locks.
/// </summary>
public interface IVKDistributedLockProvider
{
    /// <summary>
    /// Creates a lock instance for the specified resource.
    /// </summary>
    IVKDistributedLock CreateLock(string resourceKey, TimeSpan? expiryTime = null);
}
