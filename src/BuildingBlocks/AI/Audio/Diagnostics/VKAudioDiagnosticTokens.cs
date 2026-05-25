using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKAudioDiagnosticTokens
{
    public const int AudioInitializedEventId = VKDiagnosticOffsets.AI_Audio + 1;

    public static class Metrics
    {
        public const string AudioDuration = "vk.ai.audio.duration";
    }

    public static class Tags
    {
        public const string AudioId = "vk.ai.audio.id";
    }
}
