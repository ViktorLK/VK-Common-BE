using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Afferent;

public static class VKIngressTextDiagnosticTokens
{
    public const int TextPipelineStartedEventId = VKDiagnosticOffsets.AI_Text + 1;
    public const int TextPipelineCompletedEventId = VKDiagnosticOffsets.AI_Text + 2;
}
