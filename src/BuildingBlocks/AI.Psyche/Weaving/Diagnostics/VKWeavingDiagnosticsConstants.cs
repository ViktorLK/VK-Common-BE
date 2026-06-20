using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for the Weaving Engine.
/// </summary>
public static class VKWeavingDiagnosticsConstants
{
    public static class Logs
    {
        public const int WeavingTruncated = VKDiagnosticOffsets.AI_Psyche_Weaving + 1;
        public const int WeavingAssembled = VKDiagnosticOffsets.AI_Psyche_Weaving + 2;
        public const int WeavingEmptyActive = VKDiagnosticOffsets.AI_Psyche_Weaving + 3;
    }

    public static class Metrics
    {
        public const string InputTokens = "vk.ai.psyche.weaving.input_tokens";
        public const string OutputTokens = "vk.ai.psyche.weaving.output_tokens";
    }
}
