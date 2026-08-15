namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for bulkhead concurrency isolation.
/// </summary>
public interface IVKBulkhead
{
    /// <summary>
    /// Checks if a new execution is allowed within the maximum concurrency limit.
    /// </summary>
    bool IsAllowed(string key, int maxParallelization);

    /// <summary>
    /// Acquires a concurrency execution slot for the specified key.
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
}
