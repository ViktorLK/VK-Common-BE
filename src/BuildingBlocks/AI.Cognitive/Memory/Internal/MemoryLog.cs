using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// Source-generated logger messages for the Memory feature.
/// </summary>
internal static partial class MemoryLog
{
    [LoggerMessage(
        EventId = 200,
        Level = LogLevel.Information,
        Message = "Memory entry saved: {Id}")]
    public static partial void MemoryEntrySaved(this ILogger logger, string id);

    [LoggerMessage(
        EventId = 201,
        Level = LogLevel.Information,
        Message = "Memory search completed with {Count} results for query: {Query}")]
    public static partial void MemorySearchCompleted(this ILogger logger, int count, string query);

    [LoggerMessage(
        EventId = 202,
        Level = LogLevel.Information,
        Message = "RealityLedger: Archiving fact with key {Key}")]
    public static partial void FactArchived(this ILogger logger, string key);

    [LoggerMessage(
        EventId = 203,
        Level = LogLevel.Warning,
        Message = "RealityLedger: Attempted to retrieve key {Key} from fallback ledger (Not Implemented)")]
    public static partial void LedgerNotImplemented(this ILogger logger, string key);

    [LoggerMessage(
        EventId = 204,
        Level = LogLevel.Information,
        Message = "Starting manual memory pruning and metabolism cycle...")]
    public static partial void PruningManualStarted(this ILogger logger);

    [LoggerMessage(
        EventId = 205,
        Level = LogLevel.Information,
        Message = "Manual memory pruning and metabolism cycle completed successfully. Decayed: {Decayed}, Pruned: {Pruned}, Consolidated: {Consolidated}")]
    public static partial void PruningManualCompleted(this ILogger logger, int decayed, int pruned, int consolidated);

    [LoggerMessage(
        EventId = 206,
        Level = LogLevel.Error,
        Message = "Manual memory pruning cycle failed.")]
    public static partial void PruningManualFailed(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 207,
        Level = LogLevel.Information,
        Message = "Automatic memory metabolism background cycle is disabled.")]
    public static partial void AutomaticMetabolismDisabled(this ILogger logger);

    [LoggerMessage(
        EventId = 208,
        Level = LogLevel.Information,
        Message = "Starting automatic memory metabolism background cycle with interval: {Interval} minutes")]
    public static partial void AutomaticMetabolismStarted(this ILogger logger, int interval);

    [LoggerMessage(
        EventId = 209,
        Level = LogLevel.Information,
        Message = "Triggering queued background memory metabolism...")]
    public static partial void QueuedMetabolismTriggered(this ILogger logger);

    [LoggerMessage(
        EventId = 210,
        Level = LogLevel.Warning,
        Message = "Queued background memory metabolism failed: {Error}")]
    public static partial void QueuedMetabolismFailed(this ILogger logger, string error);

    [LoggerMessage(
        EventId = 211,
        Level = LogLevel.Error,
        Message = "An error occurred during automatic memory metabolism timer delay.")]
    public static partial void AutomaticMetabolismTimerError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 212,
        Level = LogLevel.Information,
        Message = "Automatic memory metabolism background cycle stopped.")]
    public static partial void AutomaticMetabolismStopped(this ILogger logger);
}
