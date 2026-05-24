using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Chat.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class ChatDiagnostics
{
    [LoggerMessage(
        EventId = VKChatDiagnosticTokens.ChatInitializedEventId,
        Level = LogLevel.Debug,
        Message = "Chat feature initialized.")]
    public static partial void ChatInitialized(ILogger logger);
}
