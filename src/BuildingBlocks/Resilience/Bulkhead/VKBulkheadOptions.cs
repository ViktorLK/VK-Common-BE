using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Configuration options for the bulkhead concurrency isolation slice.
/// </summary>
public sealed partial record VKBulkheadOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the maximum number of concurrent in-flight executions.
    /// </summary>
    public int MaxParallelization { get; init; } = 100;

    /// <summary>
    /// Gets the maximum number of queued executions.
    /// </summary>
    public int MaxQueuedItems { get; init; } = 50;
}
