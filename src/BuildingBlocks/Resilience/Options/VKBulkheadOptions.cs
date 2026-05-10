namespace VK.Blocks.Resilience;

/// <summary>
/// Configuration options for bulkhead strategies.
/// </summary>
public sealed record VKBulkheadOptions
{
    /// <summary>
    /// Gets the maximum number of concurrent executions.
    /// </summary>
    public int MaxParallelization { get; init; } = 100;

    /// <summary>
    /// Gets the maximum number of queued executions.
    /// </summary>
    public int MaxQueuedItems { get; init; } = 50;
}
