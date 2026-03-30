using VK.Blocks.Caching.Abstractions;

namespace VK.Blocks.Caching.Providers.Memory;

/// <summary>
/// A simple in-memory lock that always succeeds. 
/// NOTE: This is NOT thread-safe for actual distributed scenario but used for consistency in standalone memory provider.
/// </summary>
internal sealed class MemoryLock : IDistributedLock
{
    public bool IsAcquired => true;
    public Task<bool> AcquireAsync(CancellationToken ct) => Task.FromResult(true);
    public Task ReleaseAsync(CancellationToken ct) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
