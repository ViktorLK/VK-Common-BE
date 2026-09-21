using System;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;
using VK.Blocks.Observability.Diagnostics.Internal;

namespace VK.Blocks.Observability.Metrics.Internal;

/// <summary>
/// Default implementation of <see cref="IVKMetricFactory"/> using <see cref="System.Diagnostics.Metrics.Meter"/>.
/// </summary>
// [AP.01] sealed
// [AP.03] Internal scoping without VK prefix
internal sealed class DefaultMetricFactory : IVKMetricFactory
{
    private readonly Meter _meter;

    public DefaultMetricFactory(Meter? meter = null)
    {
        _meter = meter ?? ObservabilityDiagnostics.Meter;
    }

    /// <inheritdoc />
    public Counter<T> CreateCounter<T>(string name, string? unit = null, string? description = null)
        where T : struct
    {
        VKGuard.NotNullOrWhiteSpace(name, nameof(name));
        return _meter.CreateCounter<T>(name, unit, description);
    }

    /// <inheritdoc />
    public Histogram<T> CreateHistogram<T>(string name, string? unit = null, string? description = null)
        where T : struct
    {
        VKGuard.NotNullOrWhiteSpace(name, nameof(name));
        return _meter.CreateHistogram<T>(name, unit, description);
    }

    /// <inheritdoc />
    public UpDownCounter<T> CreateUpDownCounter<T>(string name, string? unit = null, string? description = null)
        where T : struct
    {
        VKGuard.NotNullOrWhiteSpace(name, nameof(name));
        return _meter.CreateUpDownCounter<T>(name, unit, description);
    }

    /// <inheritdoc />
    public ObservableGauge<T> CreateObservableGauge<T>(string name, Func<T> observeValue, string? unit = null, string? description = null)
        where T : struct
    {
        VKGuard.NotNullOrWhiteSpace(name, nameof(name));
        VKGuard.NotNull(observeValue, nameof(observeValue));
        return _meter.CreateObservableGauge(name, observeValue, unit, description);
    }
}
