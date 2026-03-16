using VK.Blocks.Observability.AspNetCore.Logging;

namespace VK.Blocks.Observability.AspNetCore.Metrics;

/// <summary>
/// Defines a collector for recording HTTP request metrics.
/// </summary>
public interface IHttpMetricsCollector : IDisposable
{
    /// <summary>
    /// Records metrics based on the provided <see cref="HttpLogEntry"/>.
    /// </summary>
    /// <param name="entry">The log entry containing request details.</param>
    /// <param name="exception">An optional exception if the request failed.</param>
    void Record(HttpLogEntry entry, Exception? exception = null);
}
