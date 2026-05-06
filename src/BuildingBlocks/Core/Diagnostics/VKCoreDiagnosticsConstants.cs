using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Core;

/// <summary>
/// Centralized constants for the Core diagnostics feature.
/// Complies with Rule 6 (Diagnostics), Rule 14 (Naming), and Rule 16 (Standard Folder Structure).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static constants for diagnostics and telemetry tagging.")]
public static class VKCoreDiagnosticsConstants
{
    /// <summary>
    /// The diagnostic source name for the Core block.
    /// </summary>
    public static readonly string SourceName = VKCoreBlock.Instance.ActivitySourceName;

    /// <summary>
    /// The standard tag key for Tenant ID.
    /// </summary>
    public const string TenantIdTagName = "tenant_id";

    /// <summary>
    /// The standard tag key for Trace ID.
    /// </summary>
    public const string TraceIdTagName = "trace_id";
}
