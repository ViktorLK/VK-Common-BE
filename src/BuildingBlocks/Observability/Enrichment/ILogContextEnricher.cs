namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// Defines an abstraction for automatically pushing the active
/// <see cref="System.Diagnostics.Activity"/> trace context (TraceId, SpanId)
/// to the underlying logging context.
/// <para>
/// This abstraction is independent of specific logging libraries. Implementation
/// should be chosen based on the host-side logging provider (e.g., Serilog LogContext).
/// </para>
/// </summary>
public interface ILogContextEnricher
{
    #region Public Methods

    /// <summary>
    /// Starts a scope that enriches the log context with trace information.
    /// </summary>
    /// <returns>
    /// An <see cref="IDisposable"/> that cleans up the log context when disposed.
    /// Returns a no-op implementation if no active <see cref="System.Diagnostics.Activity"/> exists.
    /// </returns>
    IDisposable Enrich();

    #endregion
}
