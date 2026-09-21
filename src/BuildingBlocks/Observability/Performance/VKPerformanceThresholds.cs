using System;

namespace VK.Blocks.Observability;

/// <summary>
/// Configurable performance thresholds for detecting and logging slow operations.
/// Complies with Manifest §13.
/// </summary>
// [AP.01] sealed
// [AP.03] Level 1 Public API with VK prefix
public sealed record VKPerformanceThresholds
{
    /// <summary>
    /// Threshold in milliseconds for flagging a generic background or internal operation as slow.
    /// Default: 1000ms.
    /// </summary>
    public double SlowOperationMs { get; init; } = 1000;

    /// <summary>
    /// Threshold in milliseconds for flagging a database query as slow.
    /// Default: 200ms.
    /// </summary>
    public double SlowQueryMs { get; init; } = 200;

    /// <summary>
    /// Threshold in milliseconds for flagging an incoming HTTP request/response as slow.
    /// Default: 3000ms.
    /// </summary>
    public double SlowHttpResponseMs { get; init; } = 3000;

    /// <summary>
    /// Determines whether the specified duration exceeds the slow operation threshold.
    /// </summary>
    public bool IsSlowOperation(TimeSpan elapsed) => elapsed.TotalMilliseconds >= SlowOperationMs;

    /// <summary>
    /// Determines whether the specified duration exceeds the slow query threshold.
    /// </summary>
    public bool IsSlowQuery(TimeSpan elapsed) => elapsed.TotalMilliseconds >= SlowQueryMs;

    /// <summary>
    /// Determines whether the specified duration exceeds the slow HTTP response threshold.
    /// </summary>
    public bool IsSlowHttpResponse(TimeSpan elapsed) => elapsed.TotalMilliseconds >= SlowHttpResponseMs;
}
