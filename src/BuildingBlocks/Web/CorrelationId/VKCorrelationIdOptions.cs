using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.Web;

/// <summary>
/// Options for configuring request correlation tracking.
/// </summary>
public sealed record VKCorrelationIdOptions : IVKBlockOptions
{
    /// <summary>
    /// The default configuration section name for Correlation ID options.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Web:CorrelationId";

    /// <summary>
    /// Gets or sets the header name used for Correlation ID.
    /// Default is "X-Correlation-ID".
    /// </summary>
    [Required]
    [MinLength(1)]
    public string Header { get; init; } = "X-Correlation-ID";

    /// <summary>
    /// Gets or sets a value indicating whether to include the Correlation ID in the response headers.
    /// </summary>
    public bool IncludeInResponse { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use the OpenTelemetry Trace ID if available.
    /// </summary>
    public bool UseTraceIdIfAvailable { get; init; } = true;

    /// <summary>
    /// Gets or sets the property name used in the logging context.
    /// Default is "CorrelationId".
    /// </summary>
    [Required]
    [MinLength(1)]
    public string LogContextPropertyName { get; init; } = "CorrelationId";
}
