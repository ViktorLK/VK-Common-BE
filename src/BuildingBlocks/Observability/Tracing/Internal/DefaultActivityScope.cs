using System.Diagnostics;
using VK.Blocks.Core;
using VK.Blocks.Observability.Diagnostics.Internal;

namespace VK.Blocks.Observability.Tracing.Internal;

/// <summary>
/// Default implementation of <see cref="IVKActivityScope"/> supporting hierarchical span creation.
/// Complies with Manifest §1 and §6.
/// </summary>
// [AP.01] sealed
// [AP.03] Internal scoping without VK prefix
internal sealed class DefaultActivityScope : IVKActivityScope
{
    private readonly ActivitySource _source;
    private readonly IVKTelemetryRedactor? _redactor;

    public DefaultActivityScope(IVKTelemetryRedactor? redactor = null, ActivitySource? source = null)
    {
        _source = source ?? ObservabilityDiagnostics.Source;
        _redactor = redactor;
    }

    /// <inheritdoc />
    public IVKActivityHandle StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        VKGuard.NotNullOrWhiteSpace(name, nameof(name));
        var activity = _source.StartActivity(name, kind);
        return new DefaultActivityHandle(activity, _redactor);
    }

    /// <inheritdoc />
    public IVKActivityHandle StartActivity(string name, ActivityKind kind, ActivityContext parentContext)
    {
        VKGuard.NotNullOrWhiteSpace(name, nameof(name));
        var activity = _source.StartActivity(name, kind, parentContext);
        return new DefaultActivityHandle(activity, _redactor);
    }
}
