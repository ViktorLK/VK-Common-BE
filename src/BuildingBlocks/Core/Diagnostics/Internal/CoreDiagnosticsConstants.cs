namespace VK.Blocks.Core.Diagnostics.Internal;

/// <summary>
/// Constants for Core diagnostics.
/// Follows OpenTelemetry semantic conventions and VK.Blocks standards.
/// </summary>
internal static class CoreDiagnosticsConstants
{
    /// <summary>
    /// The diagnostic source name for the Core block.
    /// </summary>
    public static readonly string SourceName = VKCoreBlock.Instance.ActivitySourceName;

    // Currently no Core-specific metrics or tags defined.
    // Placeholders for standard tag keys if needed globally.
    public const string TenantIdTagName = "tenant_id";
    public const string TraceIdTagName = "trace_id";
}
