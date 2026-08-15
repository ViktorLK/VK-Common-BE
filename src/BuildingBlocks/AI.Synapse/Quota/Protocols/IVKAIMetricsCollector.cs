using System;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Collects and computes performance metrics for AI connections.
/// </summary>
public interface IVKAIMetricsCollector
{
    /// <summary>
    /// Records the execution details (tokens, latency) of a completed request.
    /// </summary>
    void RecordMetrics(VKAIConnection connection, int tokens, TimeSpan latency);

    /// <summary>
    /// Gets the current exponential moving average latency (in milliseconds) for a connection.
    /// </summary>
    double GetAverageLatencyMs(VKAIConnection connection);
}
