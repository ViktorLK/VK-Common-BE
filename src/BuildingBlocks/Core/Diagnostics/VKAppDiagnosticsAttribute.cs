using System;

namespace VK.Blocks.Core.Diagnostics;

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
    /// Gets or sets the application version. Defaults to "1.0.0".
    /// </summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    /// Gets or sets a description for the application's telemetry.
    /// </summary>
    public string? Description { get; init; }
}
