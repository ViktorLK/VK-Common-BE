using System;
using System.Diagnostics.Metrics;

namespace VK.Blocks.Observability;

/// <summary>
/// Factory for creating named metrics instruments with namespace management
/// and dimension validation. Returns BCL <see cref="System.Diagnostics.Metrics"/> types.
/// Complies with Manifest §8 (High-Cardinality Dimension Prohibition).
/// </summary>
// [AP.03] Level 1 Public API
public interface IVKMetricFactory
{
    /// <summary>
    /// Creates a named <see cref="Counter{T}"/>.
    /// </summary>
    Counter<T> CreateCounter<T>(string name, string? unit = null, string? description = null)
        where T : struct;

    /// <summary>
    /// Creates a named <see cref="Histogram{T}"/>.
    /// </summary>
    Histogram<T> CreateHistogram<T>(string name, string? unit = null, string? description = null)
        where T : struct;

    /// <summary>
    /// Creates a named <see cref="UpDownCounter{T}"/>.
    /// </summary>
    UpDownCounter<T> CreateUpDownCounter<T>(string name, string? unit = null, string? description = null)
        where T : struct;

    /// <summary>
    /// Creates a named <see cref="ObservableGauge{T}"/>.
    /// </summary>
    ObservableGauge<T> CreateObservableGauge<T>(string name, Func<T> observeValue, string? unit = null, string? description = null)
        where T : struct;
}
