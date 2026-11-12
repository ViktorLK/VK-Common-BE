using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKPromptingDiagnosticsConstants
{
    public const int PromptingInitializedEventId = VKAIDiagnosticOffsets.Prompting + 1;
    public const int PromptNotFoundEventId = VKAIDiagnosticOffsets.Prompting + 2;

    public static class Metrics
    {
        public const string PromptingDuration = "vk.ai.prompting.duration";
    }

    public static class Tags
    {
        public const string PromptingId = "vk.ai.prompting.id";
    }
}
