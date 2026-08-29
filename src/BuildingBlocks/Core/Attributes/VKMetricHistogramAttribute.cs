using System;

namespace VK.Blocks.Core;

/// <summary>
/// Source-generator attribute that marks a partial method to automatically create and record a <see cref="System.Diagnostics.Metrics.Histogram{T}"/>.
/// Follows BB.04 and OR.01.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class VKMetricHistogramAttribute : Attribute
{
    /// <summary>
    /// Gets the metric instrument name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the measurement unit (e.g., "ms", "bytes", "tokens").
    /// </summary>
    public string? Unit { get; init; }

    /// <summary>
    /// Gets or sets the human-readable description of the metric.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="VKMetricHistogramAttribute"/>.
    /// </summary>
    /// <param name="name">The metric name in dot/underscore notation (e.g. "vk.ai.psyche.pipeline.duration").</param>
    public VKMetricHistogramAttribute(string name)
    {
        Name = name;
    }
}
