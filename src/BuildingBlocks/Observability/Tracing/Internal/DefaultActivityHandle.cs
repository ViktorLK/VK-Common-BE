using System;
using System.Diagnostics;
using VK.Blocks.Core;

namespace VK.Blocks.Observability.Tracing.Internal;

/// <summary>
/// Default implementation of <see cref="IVKActivityHandle"/>.
/// Wraps an <see cref="System.Diagnostics.Activity"/> and provides fluent tag and result recording.
/// </summary>
// [AP.01] sealed
// [AP.03] Internal scoping without VK prefix
internal sealed class DefaultActivityHandle : IVKActivityHandle
{
    private readonly Activity? _activity;
    private readonly IVKTelemetryRedactor? _redactor;

    public DefaultActivityHandle(Activity? activity, IVKTelemetryRedactor? redactor = null)
    {
        _activity = activity;
        _redactor = redactor;
    }

    /// <inheritdoc />
    public Activity? Activity => _activity;

    /// <inheritdoc />
    public IVKActivityHandle SetTag(string key, object? value)
    {
        VKGuard.NotNullOrWhiteSpace(key, nameof(key));

        var processedValue = _redactor is not null ? _redactor.Redact(key, value) : value;
        _activity?.SetTag(key, processedValue);
        return this;
    }

    /// <inheritdoc />
    public IVKActivityHandle RecordResult(IVKResult result)
    {
        VKGuard.NotNull(result, nameof(result));
        _activity?.RecordResult(result);
        return this;
    }

    /// <inheritdoc />
    public IVKActivityHandle RecordException(Exception exception)
    {
        VKGuard.NotNull(exception, nameof(exception));
        _activity?.RecordException(exception);
        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _activity?.Dispose();
    }
}
