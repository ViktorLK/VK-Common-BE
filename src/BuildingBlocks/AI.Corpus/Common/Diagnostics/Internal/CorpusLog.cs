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
}
