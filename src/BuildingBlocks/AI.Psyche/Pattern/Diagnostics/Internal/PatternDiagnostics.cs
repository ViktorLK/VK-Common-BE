using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages and metrics for Psyche Pattern stage.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class PatternDiagnostics
{
    // --- Source Generated Metrics (v1.5) ---

    [VKMetricHistogram(
        VKPatternDiagnosticsConstants.Metrics.PatternStageDuration,
        Unit = "ms",
        Description = "Duration of pattern resolution and injection in milliseconds.")]
    public static partial void RecordPatternStage(
        double durationMs,
        [VKMetricTag(VKPatternDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKPsycheDiagnosticsConstants.Tags.IsSuccess)] bool success);

    [VKMetricCounter(
        VKPatternDiagnosticsConstants.Metrics.PatternsResolvedCount,
        Unit = "patterns",
        Description = "Total number of patterns resolved and injected into prompt context.")]
    public static partial void RecordPatternsResolved(
        long count,
        [VKMetricTag(VKPatternDiagnosticsConstants.Tags.StageName)] string stage);

    // --- [LoggerMessage] Generators (OR.01) ---

    [LoggerMessage(
        EventId = VKPatternDiagnosticsConstants.Logs.PatternInitialized,
        Level = LogLevel.Information,
        Message = "Pattern provider initialized.")]
    public static partial void PatternInitialized(this ILogger logger);

    [LoggerMessage(
        EventId = VKPatternDiagnosticsConstants.Logs.PatternResolved,
        Level = LogLevel.Debug,
        Message = "Resolved Pattern {PatternId}.")]
    public static partial void PatternResolved(this ILogger logger, string patternId);
}
