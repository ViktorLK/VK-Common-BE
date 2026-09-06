using System.Diagnostics;

namespace VK.Blocks.Infrastructure.Observability;

/// <summary>
/// Utility to enrich structured logs and activities with cross-cutting metadata.
/// Implements OR.01 requirements.
/// </summary>
public static class VKActivityEnricher
{
    /// <summary>
    /// Adds standard VK tags to the current activity.
    /// </summary>
    public static void EnrichCurrentActivity(string key, string value)
    {
        Activity.Current?.SetTag(key, value);
    }

    /// <summary>
    /// Automatically captures the TraceId for logging contexts.
    /// </summary>
    public static string? GetTraceId() => Activity.Current?.TraceId.ToHexString();
}
