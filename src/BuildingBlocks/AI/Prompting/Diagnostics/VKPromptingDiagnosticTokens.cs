using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKPromptingDiagnosticTokens
{
    public const int PromptingInitializedEventId = VKDiagnosticOffsets.AI_Prompting + 1;

    public static class Metrics
    {
        public const string PromptingDuration = "vk.ai.prompting.duration";
    }

    public static class Tags
    {
        public const string PromptingId = "vk.ai.prompting.id";
    }
}
