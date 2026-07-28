using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Engram.Reclamation.Diagnostics.Internal;

internal static partial class ReclamationDiagnostics
{
    [LoggerMessage(
        EventId = VKReclamationDiagnosticsConstants.ReclamationCycleStartedEventId,
        Level = LogLevel.Information,
        Message = "Starting memory reclamation cycle.")]
    public static partial void ReclamationCycleStarted(this ILogger logger);

    [LoggerMessage(
        EventId = VKReclamationDiagnosticsConstants.ReclamationCycleCompletedEventId,
        Level = LogLevel.Information,
        Message = "Reclamation cycle completed. Evaluated: {Evaluated}, Decayed: {Decayed}, Pruned: {Pruned}, VectorCleaned: {VectorCleaned}")]
    public static partial void ReclamationCycleCompleted(this ILogger logger, int evaluated, int decayed, int pruned, int vectorCleaned);

    [LoggerMessage(
        EventId = VKReclamationDiagnosticsConstants.ReclamationCycleErrorEventId,
        Level = LogLevel.Warning,
        Message = "Error occurred during memory reclamation cycle.")]
    public static partial void ReclamationCycleError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = VKReclamationDiagnosticsConstants.ReclamationDecayEvaluatedEventId,
        Level = LogLevel.Debug,
        Message = "Evaluated decay for {Count} entries.")]
    public static partial void ReclamationDecayEvaluated(this ILogger logger, int count);

    [LoggerMessage(
        EventId = VKReclamationDiagnosticsConstants.ReclamationPruneExecutedEventId,
        Level = LogLevel.Information,
        Message = "Pruned memory entry {Id} with action {Action}.")]
    public static partial void ReclamationPruneExecuted(this ILogger logger, VKMemoryId id, VKPruneAction action);

    [LoggerMessage(
        EventId = VKReclamationDiagnosticsConstants.ReclamationVectorStoreCleanedEventId,
        Level = LogLevel.Information,
        Message = "Cleaned {Count} vector embeddings from VectorStore for pruned entry {Id}.")]
    public static partial void ReclamationVectorStoreCleaned(this ILogger logger, int count, VKMemoryId id);
}
