using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// Source-generated logger messages for the Chat feature.
/// </summary>
internal static partial class ChatLog
{
    [LoggerMessage(
        EventId = 100,
        Level = LogLevel.Information,
        Message = "Chat engine initialized with model: {Model}")]
    public static partial void ChatEngineInitialized(ILogger logger, string? model);

    [LoggerMessage(
        EventId = 101,
        Level = LogLevel.Warning,
        Message = "Chat request failed: {Reason}")]
    public static partial void ChatRequestFailed(ILogger logger, string reason);
}
