using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Corpus.Diagnostics.Internal;

/// <summary>
/// Source-generated logging helper class for the AI.Corpus module.
/// Follows OR.01.
/// </summary>
internal static partial class CorpusLog
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Failed to record injections for session {SessionId}. Error: {Error}")]
    public static partial void FailedToRecordInjections(ILogger logger, string sessionId, string error);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Failed to deserialize lifecycle metadata for document {DocumentId}. Using defaults.")]
    public static partial void FailedToDeserializeLifecycle(ILogger logger, string documentId, System.Exception ex);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "Gathering completed: {CandidateCount} candidates recalled for session {SessionId}")]
    public static partial void GatheringCompleted(ILogger logger, int candidateCount, string sessionId);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "Filtering completed: {PassedCount}/{TotalCount} entries passed for session {SessionId}")]
    public static partial void FilteringCompleted(ILogger logger, int passedCount, int totalCount, string sessionId);

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "Tracking recorded {InjectionCount} injections for session {SessionId} at turn {Turn}")]
    public static partial void TrackingRecorded(ILogger logger, int injectionCount, string sessionId, int turn);
}
