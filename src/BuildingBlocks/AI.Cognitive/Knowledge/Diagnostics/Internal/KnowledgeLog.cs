using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

/// <summary>
/// Source-generated logger messages for the Knowledge feature.
/// </summary>
internal static partial class KnowledgeLog
{
    [LoggerMessage(
        EventId = 1101,
        Level = LogLevel.Debug,
        Message = "Archived fact with key '{Key}' into the knowledge.")]
    public static partial void FactArchived(this ILogger logger, string key);

    [LoggerMessage(
        EventId = 1102,
        Level = LogLevel.Warning,
        Message = "Knowledge retrieval for key '{Key}' is not yet implemented in this provider.")]
    public static partial void LedgerNotImplemented(this ILogger logger, string key);
}
