namespace VK.Blocks.Caching.DistributedLock.Internal.Providers.Memory;

/// <summary>
/// Simple internal in-memory lock provider.
/// </summary>
internal sealed class MemoryDistributedLockProvider : IVKDistributedLockProvider
{
    public IVKDistributedLock CreateLock(string resourceKey, TimeSpan? expiry = null) =>
        new MemoryLock(resourceKey);
}
