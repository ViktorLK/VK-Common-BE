using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Diagnostics.Internal;

[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class MemoryDiagnostics
{
    [LoggerMessage(
        EventId = VKMemoryDiagnosticTokens.MemorySummarizationFailedEventId,
        Level = LogLevel.Error,
        Message = "Error summarizing memory content.")]
    public static partial void MemorySummarizationFailed(ILogger logger, Exception ex);
}
