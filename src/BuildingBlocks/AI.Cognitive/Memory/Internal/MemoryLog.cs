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
}
