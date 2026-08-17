using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Concurrency.Internal;

internal sealed class DefaultJobConcurrencyLimiter : IVKJobConcurrencyLimiter
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    private sealed class SemaphoreReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public SemaphoreReleaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore.Release();
                _disposed = true;
            }
        }
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Returned to caller for resource lifecycle management.")]
    public async Task<VKResult<IDisposable>> AcquireLockAsync(string jobType, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(jobType);

        var semaphore = _semaphores.GetOrAdd(jobType, _ => new SemaphoreSlim(10, 10));
        await semaphore.WaitAsync(ct).ConfigureAwait(false);

        IDisposable releaser = new SemaphoreReleaser(semaphore);
        return VKResult.Success(releaser);
    }
}
