using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for the Weaving Engine.
/// </summary>
public static class VKWeavingDiagnostics
{
    public const int WeavingTruncatedEventId = VKDiagnosticOffsets.AI_Cognitive_Weaving + 1;
    public const int WeavingAssembledEventId = VKDiagnosticOffsets.AI_Cognitive_Weaving + 2;
    public const int WeavingEmptyActiveEventId = VKDiagnosticOffsets.AI_Cognitive_Weaving + 3;

    public static class Metrics
    {
        public const string InputTokens = "vk.ai.psyche.weaving.input_tokens";
        public const string OutputTokens = "vk.ai.psyche.weaving.output_tokens";
    }
}
