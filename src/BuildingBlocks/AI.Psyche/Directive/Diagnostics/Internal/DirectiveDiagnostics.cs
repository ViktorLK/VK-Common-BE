using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages and metrics for Psyche Directive stage.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class DirectiveDiagnostics
{
    // --- Source Generated Metrics (v1.5) ---

    [VKMetricHistogram(
        VKDirectiveDiagnosticsConstants.Metrics.DirectiveStageDuration,
        Unit = "ms",
        Description = "Duration of directive resolution and injection in milliseconds.")]
    public static partial void RecordDirectiveStage(
        double durationMs,
        [VKMetricTag(VKDirectiveDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKPsycheDiagnosticsConstants.Tags.IsSuccess)] bool success);

    [VKMetricCounter(
        VKDirectiveDiagnosticsConstants.Metrics.DirectivesResolvedCount,
        Unit = "directives",
        Description = "Total number of directives resolved and injected into prompt context.")]
    public static partial void RecordDirectivesResolved(
        long count,
        [VKMetricTag(VKDirectiveDiagnosticsConstants.Tags.StageName)] string stage);

    // --- [LoggerMessage] Generators (OR.01) ---

    [LoggerMessage(
        EventId = VKDirectiveDiagnosticsConstants.Logs.DirectiveInitialized,
        Level = LogLevel.Information,
        Message = "Directive provider initialized.")]
    public static partial void DirectiveInitialized(this ILogger logger);

    [LoggerMessage(
        EventId = VKDirectiveDiagnosticsConstants.Logs.DirectiveResolved,
        Level = LogLevel.Information,
        Message = "Resolved Directive {DirectiveId}.")]
    public static partial void DirectiveResolved(this ILogger logger, string directiveId);
}
