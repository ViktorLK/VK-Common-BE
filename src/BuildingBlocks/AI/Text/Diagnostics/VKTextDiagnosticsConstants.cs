using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKTextDiagnosticsConstants
{
    public const int TextInitializedEventId = VKAIDiagnosticOffsets.Text + 1;

    public static class Metrics
    {
        public const string TextDuration = "vk.ai.text.duration";
    }

    public static class Tags
    {
        public const string TextId = "vk.ai.text.id";
    }
}
