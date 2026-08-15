using System;
using System.Collections.Concurrent;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Bulkhead.Internal;

// [AP.01] sealed
internal sealed class LocalBulkhead : IVKBulkhead
{
    private sealed class BulkheadSlot
    {
        public int InFlightCount { get; set; }
        public object LockObject { get; } = new();
    }

    private readonly ConcurrentDictionary<string, BulkheadSlot> _slots = new();

    public bool IsAllowed(string key, int maxParallelization)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var slot = _slots.GetOrAdd(key, _ => new BulkheadSlot());

        lock (slot.LockObject)
        {
            return maxParallelization <= 0 || slot.InFlightCount < maxParallelization;
        }
    }

    public void Acquire(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var slot = _slots.GetOrAdd(key, _ => new BulkheadSlot());

        lock (slot.LockObject)
        {
            slot.InFlightCount++;
        }
    }

    public void Release(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var slot = _slots.GetOrAdd(key, _ => new BulkheadSlot());

        lock (slot.LockObject)
        {
            slot.InFlightCount = Math.Max(0, slot.InFlightCount - 1);
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
}
