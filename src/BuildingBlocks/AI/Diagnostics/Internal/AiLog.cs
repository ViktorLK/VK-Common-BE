using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Diagnostics.Internal;

/// <summary>
/// Diagnostics for the AI building block.
/// </summary>
[VKBlockDiagnostics<VKAIBlock>]
internal static partial class AiLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "AI Block initialized for {BlockName}")]
    internal static partial void LogAIBlockInitialized(this ILogger logger, string blockName);
}
