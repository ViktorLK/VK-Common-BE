using System;

namespace VK.Blocks.AI.Gateway.Internal;

/// <summary>
/// Collects and computes performance metrics for AI providers.
/// </summary>
internal interface IAIMetricsCollector
{
    /// <summary>
    /// Records the execution details (tokens, latency) of a completed request.
    /// </summary>
    void RecordMetrics(IVKAIProviderOptions config, int tokens, TimeSpan latency);

    /// <summary>
    /// Gets the current exponential moving average latency (in milliseconds) for a provider.
    /// </summary>
    double GetAverageLatencyMs(IVKAIProviderOptions config);
}
