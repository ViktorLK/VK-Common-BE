using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Diagnostics.Internal;

/// <summary>
/// Diagnostics for the AI Cognitive building block.
/// </summary>
[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class VKAICognitiveLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "AI Cognitive Block initialized for {BlockName}")]
    internal static partial void LogVKAICognitiveBlockInitialized(this ILogger logger, string blockName);
}
