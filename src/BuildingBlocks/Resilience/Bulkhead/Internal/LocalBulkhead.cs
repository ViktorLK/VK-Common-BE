using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Diagnostics.Internal;

namespace VK.Blocks.Resilience.Bulkhead.Internal;

// [AP.01] sealed
internal sealed class LocalBulkhead : IVKBulkhead
{
    private sealed class BulkheadSlot
    {
        public SemaphoreSlim Semaphore { get; }
        public int MaxParallelism { get; }
        public int InFlightCount { get; set; }
        public int QueueCount { get; set; }
        public object LockObject { get; } = new();

        public BulkheadSlot(int maxParallelism)
        {
            MaxParallelism = maxParallelism;
            Semaphore = new SemaphoreSlim(maxParallelism, maxParallelism);
        }
    }

    private readonly ConcurrentDictionary<string, BulkheadSlot> _slots = new();
    private readonly VKBulkheadOptions _options;

    public LocalBulkhead(VKBulkheadOptions options)
    {
        _options = VKGuard.NotNull(options);
    }

    public bool IsAllowed(string key, int maxParallelization)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var effectiveLimit = maxParallelization > 0 ? maxParallelization : _options.MaxParallelization;
        var slot = _slots.GetOrAdd(key, _ => new BulkheadSlot(effectiveLimit));

        lock (slot.LockObject)
        {
            return slot.InFlightCount < effectiveLimit;
        }
    }

    public async Task<VKResult> AcquireAsync(
        string key,
        int? maxParallelization = null,
        int? maxQueuedCount = null,
        TimeSpan? queueTimeout = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        var limit = maxParallelization ?? _options.MaxParallelization;
        var maxQueue = maxQueuedCount ?? _options.MaxQueuedItems;
        var timeout = queueTimeout ?? TimeSpan.FromSeconds(10);

        var slot = _slots.GetOrAdd(key, _ => new BulkheadSlot(limit));

        bool canQueue = false;
        lock (slot.LockObject)
        {
            if (slot.InFlightCount < limit)
            {
                slot.InFlightCount++;
                ResilienceDiagnostics.RecordStrategyExecution("bulkhead", true);
                return VKResult.Success();
            }

            if (slot.QueueCount < maxQueue)
            {
                slot.QueueCount++;
                canQueue = true;
            }
        }

        if (!canQueue)
        {
            ResilienceDiagnostics.RecordStrategyExecution("bulkhead", false);
            return VKResult.Failure(VKResilienceErrors.BulkheadExceeded);
        }

        try
        {
            var acquired = await slot.Semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
            lock (slot.LockObject)
            {
                slot.QueueCount = Math.Max(0, slot.QueueCount - 1);
                if (acquired)
                {
                    slot.InFlightCount++;
                }
            }

            if (!acquired)
            {
                ResilienceDiagnostics.RecordStrategyExecution("bulkhead", false);
                return VKResult.Failure(VKResilienceErrors.BulkheadExceeded);
            }

            ResilienceDiagnostics.RecordStrategyExecution("bulkhead", true);
            return VKResult.Success();
        }
        catch (OperationCanceledException)
        {
            lock (slot.LockObject)
            {
                slot.QueueCount = Math.Max(0, slot.QueueCount - 1);
            }
            throw;
        }
    }

    public void Acquire(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var slot = _slots.GetOrAdd(key, _ => new BulkheadSlot(_options.MaxParallelization));

        lock (slot.LockObject)
        {
            slot.InFlightCount++;
        }
    }

    public void Release(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        if (_slots.TryGetValue(key, out var slot))
        {
            lock (slot.LockObject)
            {
                slot.InFlightCount = Math.Max(0, slot.InFlightCount - 1);
            }

            try
            {
                slot.Semaphore.Release();
            }
            catch (SemaphoreFullException)
            {
                // Concurrency count at max capacity
            }
        }
    }

    public int GetInFlightCount(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        if (_slots.TryGetValue(key, out var slot))
        {
            lock (slot.LockObject)
            {
                return slot.InFlightCount;
            }
        }

        return 0;
    }

    public int GetQueueCount(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        if (_slots.TryGetValue(key, out var slot))
        {
            lock (slot.LockObject)
            {
                return slot.QueueCount;
            }
        }

        return 0;
    }
}
