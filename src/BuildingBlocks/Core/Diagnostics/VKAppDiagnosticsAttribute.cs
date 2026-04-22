using System;

using VK.Blocks.Core.Constants.Internal;

namespace VK.Blocks.Core;

/// <summary>
/// Lightweight diagnostic attribute for external applications or laboratory projects 
/// that do not implement IVKBlockMarker.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class VKAppDiagnosticsAttribute(string appName) : Attribute
{
    /// <summary>
    /// Gets the application name for diagnostics.
    /// </summary>
    public string AppName { get; } = appName;

    /// <summary>
    /// Gets or sets the application version. Defaults to <see cref="CoreConstants.DefaultVersion"/>.
    /// </summary>
    public string Version { get; init; } = CoreConstants.DefaultVersion;

    /// <summary>
    /// Gets or sets a description for the application's telemetry.
    /// </summary>
    public string? Description { get; init; }
}
