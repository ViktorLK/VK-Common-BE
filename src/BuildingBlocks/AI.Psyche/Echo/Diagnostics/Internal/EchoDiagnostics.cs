using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Echo.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages and metrics for Psyche Echo feature.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class EchoDiagnostics
{
    // --- Source Generated Metrics (v1.5) ---

    [VKMetricHistogram(
        VKEchoDiagnosticsConstants.Metrics.EchoExtractDuration,
        Unit = "ms",
        Description = "Duration of dialogue echo extraction and pruning in milliseconds.")]
    public static partial void RecordExtract(
        double durationMs,
        [VKMetricTag(VKEchoDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKEchoDiagnosticsConstants.Tags.RetainedCount)] int retainedCount,
        [VKMetricTag(VKEchoDiagnosticsConstants.Tags.TrimmedCount)] int trimmedCount);

    [VKMetricCounter(
        VKEchoDiagnosticsConstants.Metrics.ActiveEchoesCount,
        Unit = "echoes",
        Description = "Total number of dialogue echoes retained in context.")]
    public static partial void RecordActiveEchoes(
        long count,
        [VKMetricTag(VKEchoDiagnosticsConstants.Tags.StageName)] string stage);

    [VKMetricCounter(
        VKEchoDiagnosticsConstants.Metrics.TrimmedEchoesCount,
        Unit = "echoes",
        Description = "Total number of dialogue echoes pruned due to turn or token limits.")]
    public static partial void RecordTrimmedEchoes(
        long count,
        [VKMetricTag(VKEchoDiagnosticsConstants.Tags.StageName)] string stage);

    [VKMetricHistogram(
        VKEchoDiagnosticsConstants.Metrics.EchoSaveDuration,
        Unit = "ms",
        Description = "Duration of dialogue echo persistence in milliseconds.")]
    public static partial void RecordSave(
        double durationMs,
        [VKMetricTag(VKEchoDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKPsycheDiagnosticsConstants.Tags.IsSuccess)] bool success);

    // --- [LoggerMessage] Generators (OR.01) ---

    [LoggerMessage(
        EventId = VKEchoDiagnosticsConstants.Logs.EchoInitialized,
        Level = LogLevel.Information,
        Message = "Echo short-term memory tracker initialized.")]
    public static partial void EchoInitialized(this ILogger logger);

    [LoggerMessage(
        EventId = VKEchoDiagnosticsConstants.Logs.EchoRecorded,
        Level = LogLevel.Debug,
        Message = "Recorded memory echo for session {SessionId}. Sender: {SenderRole}, Content length: {ContentLength}.")]
    public static partial void EchoRecorded(this ILogger logger, VKSessionId sessionId, string senderRole, int contentLength);

    [LoggerMessage(
        EventId = VKEchoDiagnosticsConstants.Logs.EchoTrimmed,
        Level = LogLevel.Information,
        Message = "Trimmed dialogue history for session {SessionId}. Original count: {OriginalCount}, Retained count: {RetainedCount}.")]
    public static partial void EchoTrimmed(this ILogger logger, VKSessionId sessionId, int originalCount, int retainedCount);
}
