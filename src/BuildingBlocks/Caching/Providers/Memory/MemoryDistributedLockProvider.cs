using VK.Blocks.Caching.Abstractions;

namespace VK.Blocks.Caching.Providers.Memory;

/// <summary>
/// Simple internal in-memory lock for standalone memory caching.
/// </summary>
internal sealed class MemoryDistributedLockProvider : IDistributedLockProvider
{
    public IDistributedLock CreateLock(string resourceKey, TimeSpan? expiry = null) => new MemoryLock();
}
