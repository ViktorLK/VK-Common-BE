using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Default single-instance in-memory lock implementation for session compression.
/// Implements auto-expiration via leaseTime and cleanup.
/// </summary>
internal sealed class InMemorySessionCompressionLock : IVKSessionCompressionLock
{
    private readonly ConcurrentDictionary<VKSessionId, SemaphoreSlim> _locks = new();

    public async Task<VKResult<IAsyncDisposable>> TryAcquireAsync(
        VKSessionId sessionId,
        TimeSpan leaseTime,
        CancellationToken cancellationToken = default)
    {
        if (sessionId.IsEmpty)
        {
            return VKResult.Failure<IAsyncDisposable>(VKCompressionErrors.InvalidSession);
        }

        var semaphore = _locks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
        bool acquired = await semaphore.WaitAsync(TimeSpan.Zero, cancellationToken).ConfigureAwait(false);

        if (!acquired)
        {
            return VKResult.Failure<IAsyncDisposable>(VKCompressionErrors.LockAcquisitionFailed);
        }

        IAsyncDisposable handle = new LockReleaser(() =>
        {
            semaphore.Release();
            if (semaphore.CurrentCount == 1)
            {
                _locks.TryRemove(sessionId, out _);
            }
        }, leaseTime);

        return VKResult.Success(handle);
    }

    private sealed class LockReleaser : IAsyncDisposable
    {
        private readonly Action _onRelease;
        private readonly CancellationTokenSource _leaseCts;
        private int _disposed;

        public LockReleaser(Action onRelease, TimeSpan leaseTime)
        {
            _onRelease = onRelease;
            _leaseCts = new CancellationTokenSource(leaseTime);
            _leaseCts.Token.Register(() =>
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                {
                    _onRelease();
                }
            });
        }

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _leaseCts.Cancel();
                _onRelease();
            }
            _leaseCts.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
