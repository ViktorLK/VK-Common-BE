using System;
using System.Diagnostics;
using VK.Blocks.Core;

namespace VK.Blocks.Observability;

/// <summary>
/// Represents an active <see cref="Activity"/> handle with lifecycle and error recording capabilities.
/// </summary>
// [AP.03] Level 1 Public API
public interface IVKActivityHandle : IDisposable
{
    /// <summary>
    /// Gets the underlying <see cref="Activity"/>, or <c>null</c> if no activity was started.
    /// </summary>
    Activity? Activity { get; }

    /// <summary>
    /// Sets a tag on the underlying activity, applying telemetry redaction when applicable.
    /// </summary>
    IVKActivityHandle SetTag(string key, object? value);

    /// <summary>
    /// Records the content and status of an <see cref="IVKResult"/> into the span.
    /// </summary>
    IVKActivityHandle RecordResult(IVKResult result);

    /// <summary>
    /// Records an exception into the span.
    /// </summary>
    IVKActivityHandle RecordException(Exception exception);
}
