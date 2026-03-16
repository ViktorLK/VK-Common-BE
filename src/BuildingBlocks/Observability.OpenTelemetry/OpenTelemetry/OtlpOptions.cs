using Microsoft.Extensions.Options;

namespace VK.Blocks.Observability.OpenTelemetry;

/// <summary>
/// Configuration options for OpenTelemetry OTLP exporter.
/// </summary>
public sealed class OtlpOptions
{
    #region Constants

    /// <summary>
    /// The configuration section name for OTLP options.
    /// </summary>
    public const string SectionName = "Otlp";

    #endregion

    #region Properties

    /// <summary>
    /// The OTLP endpoint URL. Default is "http://localhost:4317".
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// The service name to identify this application in traces and metrics.
    /// </summary>
    public string ServiceName { get; set; } = OtlpOptionsConstants.DefaultServiceName;

    /// <summary>
    /// The service version.
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Enable tracing export to OTLP. Default is true.
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Enable metrics export to OTLP. Default is true.
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Enable console exporter for debugging based on Environment.
    /// </summary>
    public bool EnableConsoleExporter { get; set; } = false;

    /// <summary>
    /// Custom headers for OTLP exporter (e.g., for authentication).
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    #endregion
}
