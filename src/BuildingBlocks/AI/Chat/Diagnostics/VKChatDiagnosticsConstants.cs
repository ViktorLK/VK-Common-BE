using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKChatDiagnosticsConstants
{
    public const int ChatInitializedEventId = VKAIDiagnosticOffsets.Chat + 1;
    public const int ChatRequestStartedEventId = VKAIDiagnosticOffsets.Chat + 2;
    public const int ChatRequestCompletedEventId = VKAIDiagnosticOffsets.Chat + 3;
    public const int ChatRequestFailedEventId = VKAIDiagnosticOffsets.Chat + 4;
    public const int ChatRequestTimedOutEventId = VKAIDiagnosticOffsets.Chat + 5;
    public const int UnexpectedChatErrorEventId = VKAIDiagnosticOffsets.Chat + 6;

    public static class Metrics
    {
        public const string ChatDuration = "vk.ai.chat.duration";
    }

    public static class Tags
    {
        public const string ChatId = "vk.ai.chat.id";
    }
}
