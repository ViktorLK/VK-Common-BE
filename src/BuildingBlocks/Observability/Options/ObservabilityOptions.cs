using System.ComponentModel.DataAnnotations;

namespace VK.Blocks.Observability.Options;

/// <summary>
/// Configuration options for the Observability block.
/// Manages application identification, telemetry enablement, and PII protection.
/// </summary>
public sealed class ObservabilityOptions
{
    #region Properties

    /// <summary>
    /// Gets or sets the application name.
    /// Used as the <c>service.name</c> attribute in logs and traces.
    /// </summary>
    [Required, MinLength(1)]
    public string ApplicationName { get; init; } = "Unknown";

    /// <summary>
    /// Gets or sets the service version.
    /// Used as the <c>service.version</c> attribute in logs and traces.
    /// </summary>
    [Required, MinLength(1)]
    public string ServiceVersion { get; init; } = "1.0.0";

    /// <summary>
    /// Gets or sets the deployment environment (e.g., Production, Staging, Development).
    /// </summary>
    public string Environment { get; init; } = "Production";

    /// <summary>
    /// Gets or sets a value indicating whether distributed tracing is enabled.
    /// </summary>
    public bool EnableTracing { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether metrics collection is enabled.
    /// </summary>
    public bool EnableMetrics { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include the user name (<c>enduser.name</c>) in logs.
    /// Disabled by default for PII protection.
    /// </summary>
    public bool IncludeUserName { get; init; }

    #endregion
}
