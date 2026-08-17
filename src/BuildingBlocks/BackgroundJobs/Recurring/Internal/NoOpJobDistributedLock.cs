using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Recurring.Internal;

internal sealed class NoOpJobDistributedLock : IVKJobDistributedLock
{
    private sealed class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Returned to caller for resource lifecycle management.")]
    public Task<VKResult<IDisposable?>> TryAcquireLockAsync(string resourceKey, TimeSpan timeout, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(resourceKey);
        IDisposable handle = new NoOpDisposable();
        return Task.FromResult(VKResult.Success<IDisposable?>(handle));
    }
}
