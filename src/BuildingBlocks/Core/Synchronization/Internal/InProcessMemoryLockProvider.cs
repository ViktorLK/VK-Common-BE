using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core.Synchronization.Internal;

/// <summary>
/// In-process memory fallback implementation of <see cref="IVKDistributedLockProvider"/>.
/// </summary>
internal sealed class InProcessMemoryLockProvider : IVKDistributedLockProvider
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async ValueTask<IVKDistributedLockHandle?> TryAcquireLockAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(lockKey);

        var semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        bool acquired = await semaphore.WaitAsync(TimeSpan.Zero, cancellationToken).ConfigureAwait(false);

        if (!acquired)
        {
            return null;
        }

        var cts = new CancellationTokenSource(expiry);
        return new InProcessLockHandle(semaphore, cts);
    }

    private sealed class InProcessLockHandle : IVKDistributedLockHandle
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _cts;
        private int _disposed;

        public InProcessLockHandle(SemaphoreSlim semaphore, CancellationTokenSource cts)
        {
            _semaphore = semaphore;
            _cts = cts;
        }

        public bool IsAcquired => Interlocked.CompareExchange(ref _disposed, 0, 0) == 0;

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _semaphore.Release();
                _cts.Dispose();
            }

            return ValueTask.CompletedTask;
        }
    }
}
