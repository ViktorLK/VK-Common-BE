using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Session.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages and metrics for Psyche Session stage.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class SessionDiagnostics
{
    // --- Source Generated Metrics (v1.5) ---

    [VKMetricHistogram(
        VKSessionDiagnosticsConstants.Metrics.SessionResolveDuration,
        Unit = "ms",
        Description = "Duration of session resolution in milliseconds.")]
    public static partial void RecordSessionResolve(
        double durationMs,
        [VKMetricTag(VKSessionDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKPsycheDiagnosticsConstants.Tags.IsSuccess)] bool success);

    [VKMetricCounter(
        VKSessionDiagnosticsConstants.Metrics.ActiveSessionsResolvedCount,
        Unit = "sessions",
        Description = "Total number of active sessions resolved.")]
    public static partial void RecordActiveSessionsResolved(
        long count,
        [VKMetricTag(VKSessionDiagnosticsConstants.Tags.StageName)] string stage);

    [VKMetricHistogram(
        VKSessionDiagnosticsConstants.Metrics.SessionUpdateDuration,
        Unit = "ms",
        Description = "Duration of session state and turn update in milliseconds.")]
    public static partial void RecordSessionUpdate(
        double durationMs,
        [VKMetricTag(VKSessionDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKPsycheDiagnosticsConstants.Tags.IsSuccess)] bool success);

    // --- [LoggerMessage] Generators (OR.01) ---

    [LoggerMessage(
        EventId = VKSessionDiagnosticsConstants.Logs.SessionInitialized,
        Level = LogLevel.Information,
        Message = "Session provider initialized.")]
    public static partial void SessionInitialized(this ILogger logger);

    [LoggerMessage(
        EventId = VKSessionDiagnosticsConstants.Logs.SessionResolved,
        Level = LogLevel.Debug,
        Message = "Resolved Session {SessionId}. Mode: {Mode}, TurnCount: {TurnCount}")]
    public static partial void SessionResolved(this ILogger logger, VKSessionId sessionId, string mode, int turnCount);

    [LoggerMessage(
        EventId = VKSessionDiagnosticsConstants.Logs.SessionUpdated,
        Level = LogLevel.Debug,
        Message = "Updated Session {SessionId}. New TurnCount: {TurnCount}")]
    public static partial void SessionUpdated(this ILogger logger, VKSessionId sessionId, int turnCount);

    [LoggerMessage(
        EventId = VKSessionDiagnosticsConstants.Logs.SessionNotActive,
        Level = LogLevel.Warning,
        Message = "Session {SessionId} is not in Active status ({Status}).")]
    public static partial void SessionNotActive(this ILogger logger, VKSessionId sessionId, string status);
}
