using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Prompting.Internal;

/// <summary>
/// High-performance logger messages for the Prompting feature.
/// </summary>
internal static partial class PromptingLog
{
    [LoggerMessage(
        EventId = 5100,
        Level = LogLevel.Warning,
        Message = "[AI] Prompt template '{PromptId}' (Version: {Version}) was not found in any registered provider.")]
    public static partial void PromptNotFound(ILogger logger, string promptId, string version);
}
