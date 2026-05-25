using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKTextDiagnosticTokens
{
    public const int TextInitializedEventId = VKDiagnosticOffsets.AI_Text + 1;

    public static class Metrics
    {
        public const string TextDuration = "vk.ai.text.duration";
    }

    public static class Tags
    {
        public const string TextId = "vk.ai.text.id";
    }
}
