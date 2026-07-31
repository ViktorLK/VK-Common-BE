using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Engram.Revision.Diagnostics.Internal;

internal static partial class RevisionDiagnostics
{
    [LoggerMessage(
        EventId = VKRevisionDiagnosticsConstants.RevisionArbitrationCompletedEventId,
        Level = LogLevel.Information,
        Message = "Contradiction arbitration completed. Kind: {Kind}, TargetId: {TargetId}")]
    public static partial void RevisionArbitrationCompleted(this ILogger logger, VKContradictionKind kind, string? targetId);

    [LoggerMessage(
        EventId = VKRevisionDiagnosticsConstants.RevisionArbitrationErrorEventId,
        Level = LogLevel.Warning,
        Message = "Error occurred during contradiction arbitration LLM call.")]
    public static partial void RevisionArbitrationError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = VKRevisionDiagnosticsConstants.RevisionUpdatesThrottledEventId,
        Level = LogLevel.Warning,
        Message = "Revision updates throttled for memory entry {Id} (rate limit exceeded).")]
    public static partial void RevisionUpdatesThrottled(this ILogger logger, VKMemoryId id);

    [LoggerMessage(
        EventId = VKRevisionDiagnosticsConstants.RevisionEntryUpdatedEventId,
        Level = LogLevel.Information,
        Message = "Memory entry {Id} revised to Version {Version}.")]
    public static partial void RevisionEntryUpdated(this ILogger logger, VKMemoryId id, int version);

    [LoggerMessage(
        EventId = VKRevisionDiagnosticsConstants.RevisionContradictionLoggedEventId,
        Level = LogLevel.Warning,
        Message = "Revision detected unresolved contradiction for memory entry {Id}. Content: {Content} vs Context: {Turn}")]
    public static partial void RevisionContradictionLogged(this ILogger logger, VKMemoryId id, string content, string turn);

    [LoggerMessage(
        EventId = VKRevisionDiagnosticsConstants.RevisionSkippedIdempotentEventId,
        Level = LogLevel.Information,
        Message = "Revision skipped (idempotent duplicate request) for entry {Id}.")]
    public static partial void RevisionSkippedIdempotent(this ILogger logger, VKMemoryId id);

    [LoggerMessage(
        EventId = VKRevisionDiagnosticsConstants.RevisionRejectedLowerAuthorityEventId,
        Level = LogLevel.Warning,
        Message = "Revision rejected due to lower authority weight. Request: {ReqAuth} < Existing: {ExistAuth} for entry {Id}.")]
    public static partial void RevisionRejectedLowerAuthority(this ILogger logger, float reqAuth, float existAuth, VKMemoryId id);

    [LoggerMessage(
        EventId = VKRevisionDiagnosticsConstants.RevisionRollbackCompletedEventId,
        Level = LogLevel.Information,
        Message = "Memory entry {Id} rolled back to version {TargetVersion} (new version index {NewVersion}).")]
    public static partial void RevisionRollbackCompleted(this ILogger logger, VKMemoryId id, int targetVersion, int newVersion);

    [LoggerMessage(
        EventId = VKRevisionDiagnosticsConstants.SynopsisMarkedStaleEventId,
        Level = LogLevel.Information,
        Message = "Marked Synopsis memory entry {SynopsisId} as stale due to revision of dependent entry {TargetId}.")]
    public static partial void SynopsisMarkedStale(this ILogger logger, VKMemoryId synopsisId, VKMemoryId targetId);
}
