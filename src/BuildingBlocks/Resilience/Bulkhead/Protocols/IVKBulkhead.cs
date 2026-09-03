using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for bulkhead concurrency isolation and queuing.
/// Follows [AP.01], [CS.01], [CS.03].
/// </summary>
public interface IVKBulkhead
{
    /// <summary>
    /// Checks if a new execution is allowed immediately within the maximum concurrency limit without queuing.
    /// </summary>
    bool IsAllowed(string key, int maxParallelization);

    /// <summary>
    /// Acquires an execution slot asynchronously, queuing up to <paramref name="maxQueuedCount"/> if concurrency is full.
    /// </summary>
    Task<VKResult> AcquireAsync(
        string key,
        int? maxParallelization = null,
        int? maxQueuedCount = null,
        TimeSpan? queueTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously acquires a concurrency execution slot for the specified key if capacity allows.
    /// </summary>
    void Acquire(string key);

    /// <summary>
    /// Releases a concurrency execution slot for the specified key.
    /// </summary>
    void Release(string key);

    /// <summary>
    /// Gets the current number of in-flight executions for the specified key.
    /// </summary>
    int GetInFlightCount(string key);

    /// <summary>
    /// Gets the current number of queued tasks waiting for a slot for the specified key.
    /// </summary>
    int GetQueueCount(string key);
}
