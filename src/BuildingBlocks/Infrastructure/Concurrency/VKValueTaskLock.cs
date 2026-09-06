using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Infrastructure.Concurrency;

/// <summary>
/// A lightweight, non-blocking asynchronous lock optimized for ValueTask.
/// Minimizes heap allocations and contention in high-concurrency scenarios.
/// </summary>
public sealed class VKValueTaskLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Acquires the lock asynchronously.
    /// </summary>
    public async ValueTask<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new Releaser(_semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = semaphore;
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            _semaphore.Release();
            _disposed = true;
        }
    }
}
