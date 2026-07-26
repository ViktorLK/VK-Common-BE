using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIEngramBlock>]
internal static partial class ConsolidationDiagnostics
{
    [LoggerMessage(
        EventId = VKConsolidationDiagnosticsConstants.IdempotencySkippedEventId,
        Level = LogLevel.Information,
        Message = "Consolidation skipped by IdempotencyGuard for SessionId: {SessionId}.")]
    public static partial void IdempotencySkipped(this ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = VKConsolidationDiagnosticsConstants.PoisoningGuardSkippedSizeEventId,
        Level = LogLevel.Warning,
        Message = "Memory entry skipped by PoisoningGuard: content exceeds safe length limit ({Limit} chars).")]
    public static partial void PoisoningGuardSkippedSize(this ILogger logger, int limit);

    [LoggerMessage(
        EventId = VKConsolidationDiagnosticsConstants.PoisoningGuardSkippedInjectionEventId,
        Level = LogLevel.Warning,
        Message = "Memory entry skipped by PoisoningGuard: potential prompt injection detected.")]
    public static partial void PoisoningGuardSkippedInjection(this ILogger logger);

    [LoggerMessage(
        EventId = VKConsolidationDiagnosticsConstants.ContradictionArbitratedEventId,
        Level = LogLevel.Information,
        Message = "Contradiction arbitration marked entry {OldId} as superseded by {NewId}.")]
    public static partial void ContradictionArbitrated(this ILogger logger, string oldId, string newId);

    [LoggerMessage(
        EventId = VKConsolidationDiagnosticsConstants.PersistenceFailedDlqEventId,
        Level = LogLevel.Error,
        Message = "Consolidation entry failed to persist after max attempts. Pushed to DLQFallback. EntryId: {EntryId}, ContentLength: {Length}.")]
    public static partial void PersistenceFailedDlq(this ILogger logger, string entryId, int length);

    [LoggerMessage(
        EventId = VKConsolidationDiagnosticsConstants.PersistenceRetryEventId,
        Level = LogLevel.Warning,
        Message = "Persistence attempt {Attempt} failed for EntryId: {EntryId}. Retrying...")]
    public static partial void PersistenceRetry(this ILogger logger, int attempt, string entryId);
}
