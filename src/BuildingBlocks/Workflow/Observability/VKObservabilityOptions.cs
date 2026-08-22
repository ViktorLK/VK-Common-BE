using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Options for Workflow observability and metrics reporting.
/// </summary>
public sealed partial record VKObservabilityOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets whether OpenTelemetry metrics collection is enabled.
    /// Defaults to true.
    /// </summary>
    public bool EnableMetrics { get; init; } = true;

    /// <summary>
    /// Gets or sets whether failure alerts should be dispatched.
    /// Defaults to true.
    /// </summary>
    public bool EnableAlerts { get; init; } = true;
}
