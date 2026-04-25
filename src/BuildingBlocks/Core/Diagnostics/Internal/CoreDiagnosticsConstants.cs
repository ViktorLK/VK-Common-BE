using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Core.Diagnostics.Internal;

/// <summary>
/// Constants for Core diagnostics.
/// Follows OpenTelemetry semantic conventions and VK.Blocks standards.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Constants and metadata providers do not contain testable logic.")]
internal static class CoreDiagnosticsConstants
{
    /// <summary>
    /// The diagnostic source name for the Core block.
    /// </summary>
    internal const string SourceName = VKCoreBlock.BlockIdentifier;

    // Currently no Core-specific metrics or tags defined.
    // Placeholders for standard tag keys if needed globally.
    internal const string TenantIdTagName = "tenant_id";
    internal const string TraceIdTagName = "trace_id";
}
