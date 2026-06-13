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
        Message = "Failed to record usage for corpus entry {EntryId} in session {SessionId}. Error: {Error}")]
    public static partial void FailedToRecordUsage(ILogger logger, string entryId, string sessionId, string error);
}
