using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIEngramBlock>]
internal static partial class CompressionDiagnostics
{
    // --- DefaultCompressionService ---

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.CompressionTriggeredEventId,
        Level = LogLevel.Information,
        Message = "Compression triggered: total tokens {TotalTokens}/{Budget}, turns {Turns}/{MaxTurns} (Reason: {Reason}) for session {SessionId}.")]
    public static partial void CompressionTriggered(this ILogger logger, int totalTokens, int budget, int turns, int maxTurns, string reason, string sessionId);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.CompressionCompletedEventId,
        Level = LogLevel.Information,
        Message = "Compression completed successfully. Original tokens: {OriginalTokens}, Compressed tokens: {CompressedTokens}.")]
    public static partial void CompressionCompleted(this ILogger logger, int originalTokens, int compressedTokens);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.CompressionSkippedEventId,
        Level = LogLevel.Debug,
        Message = "Compression skipped: total tokens {TotalTokens}/{Budget}, turns {Turns}/{MaxTurns} for session {SessionId}.")]
    public static partial void CompressionSkipped(this ILogger logger, int totalTokens, int budget, int turns, int maxTurns, string sessionId);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.CompressionFailedEventId,
        Level = LogLevel.Error,
        Message = "Compression failed for session {SessionId}: {Error}.")]
    public static partial void CompressionFailed(this ILogger logger, string sessionId, string error);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.L2CompressionTriggeredEventId,
        Level = LogLevel.Information,
        Message = "L2 summary token length {Tokens} exceeds budget {Budget} for session {SessionId}. Compressing combined summary.")]
    public static partial void L2CompressionTriggered(this ILogger logger, int tokens, int budget, string sessionId);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.CompressionSkippedLockBusyEventId,
        Level = LogLevel.Debug,
        Message = "Compression skipped for session {SessionId}: lock currently held by another worker.")]
    public static partial void CompressionSkippedLockBusy(this ILogger logger, string sessionId);

    // --- DefaultCompressionStage ---

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.JobEnqueuedEventId,
        Level = LogLevel.Information,
        Message = "Enqueued compression job for session {SessionId} asynchronously (Tokens: {Tokens}/{Budget}, Turns: {Turns}/{MaxTurns}).")]
    public static partial void JobEnqueued(this ILogger logger, string sessionId, int tokens, int budget, int turns, int maxTurns);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.QueueFullEventId,
        Level = LogLevel.Warning,
        Message = "Failed to enqueue compression job for session {SessionId} (queue full).")]
    public static partial void QueueFull(this ILogger logger, string sessionId);

    // --- DefaultCompressionBackgroundService ---

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.WorkerDisabledEventId,
        Level = LogLevel.Information,
        Message = "Automatic compression background worker is disabled.")]
    public static partial void WorkerDisabled(this ILogger logger);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.WorkerStartedEventId,
        Level = LogLevel.Information,
        Message = "Automatic compression background worker started. Interval: {IntervalMinutes} minutes.")]
    public static partial void WorkerStarted(this ILogger logger, int intervalMinutes);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.WorkerStoppedEventId,
        Level = LogLevel.Information,
        Message = "Automatic compression background worker stopped.")]
    public static partial void WorkerStopped(this ILogger logger);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.CycleStartingEventId,
        Level = LogLevel.Information,
        Message = "Starting background compression cycle...")]
    public static partial void CycleStarting(this ILogger logger);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.CycleCompletedEventId,
        Level = LogLevel.Information,
        Message = "Background compression cycle completed.")]
    public static partial void CycleCompleted(this ILogger logger);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.SessionsFoundEventId,
        Level = LogLevel.Information,
        Message = "Found {Count} active sessions for background compression.")]
    public static partial void SessionsFound(this ILogger logger, int count);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.CycleErrorEventId,
        Level = LogLevel.Error,
        Message = "An error occurred during the automatic compression background cycle.")]
    public static partial void CycleError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.SearchFailedEventId,
        Level = LogLevel.Error,
        Message = "Failed to search echoes for active sessions: {Errors}")]
    public static partial void SearchFailed(this ILogger logger, string errors);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.SessionCompressionFailedEventId,
        Level = LogLevel.Warning,
        Message = "Background compression failed for session {SessionId}: {Errors}")]
    public static partial void SessionCompressionFailed(this ILogger logger, VKSessionId sessionId, string errors);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.SessionExceptionEventId,
        Level = LogLevel.Error,
        Message = "Unhandled exception during background compression for session {SessionId}")]
    public static partial void SessionException(this ILogger logger, VKSessionId sessionId, Exception ex);

    // --- TopicSegmentationCompressionStrategy ---

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.TopicSegmentationFailedEventId,
        Level = LogLevel.Warning,
        Message = "Failed to detect topic boundaries: {Error}. Falling back to single summarization.")]
    public static partial void TopicSegmentationFailed(this ILogger logger, string error);

    [LoggerMessage(
        EventId = VKCompressionDiagnosticsConstants.NoValidSegmentsParsedEventId,
        Level = LogLevel.Warning,
        Message = "No valid segments parsed. Falling back to single summarization.")]
    public static partial void NoValidSegmentsParsed(this ILogger logger);
}
