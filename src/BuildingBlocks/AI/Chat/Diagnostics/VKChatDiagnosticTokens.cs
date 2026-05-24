using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKChatDiagnosticTokens
{
    public const int ChatInitializedEventId = VKDiagnosticOffsets.AI_Chat + 1;

    public static class Metrics
    {
        public const string ChatDuration = "vk.ai.chat.duration";
    }

    public static class Tags
    {
        public const string ChatId = "vk.ai.chat.id";
    }
}
