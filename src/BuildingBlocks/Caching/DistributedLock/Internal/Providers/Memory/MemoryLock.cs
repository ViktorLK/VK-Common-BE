namespace VK.Blocks.Caching.DistributedLock.Internal.Providers.Memory;

/// <summary>
/// A simple in-memory lock that always succeeds.
/// </summary>
internal sealed class MemoryLock(string resourceKey) : IVKDistributedLock
{
    public string ResourceKey => resourceKey;

    public Task<bool> AcquireAsync(CancellationToken ct = default) => Task.FromResult(true);

    public Task ReleaseAsync(CancellationToken ct = default) => Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
