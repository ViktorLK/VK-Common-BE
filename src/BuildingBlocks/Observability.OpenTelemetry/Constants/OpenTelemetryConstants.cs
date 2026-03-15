namespace VK.Blocks.Observability.OpenTelemetry.Constants;

/// <summary>
/// Contains constants used across the OpenTelemetry module.
/// </summary>
public static class OpenTelemetryConstants
{
    /// <summary>
    /// The wildcard source name used for tracing and metrics collection across VK.Blocks.
    /// </summary>
    public const string VkBlocksWildcardSource = "VK.Blocks.*";

    /// <summary>
    /// The ActivitySource name for Entity Framework Core.
    /// </summary>
    public const string EfCoreActivitySourceName = "Microsoft.EntityFrameworkCore";
    
    /// <summary>
    /// Path segments excluded from OpenTelemetry instrumentation tracking.
    /// </summary>
    public static readonly string[] ExcludedHealthPaths = ["/health", "/healthz", "/ready"];
}
