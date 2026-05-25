using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Weaving feature.
/// </summary>
public static class VKWeavingDiagnosticTokens
{
    // Logs (Event IDs)
    public const int WeavingInitializedEventId = VKDiagnosticOffsets.AI_Cognitive_Weaving + 1;
    public const int TokenLimitWarningEventId = VKDiagnosticOffsets.AI_Cognitive_Weaving + 2;
    public const int TapestryWeavedEventId = VKDiagnosticOffsets.AI_Cognitive_Weaving + 3;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string WeavingDuration = "vk.ai.cognitive.weaving.duration";
        public const string TokensPruned = "vk.ai.cognitive.weaving.tokens.pruned";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string TierType = "vk.ai.weaving.tier_type";
        public const string IsTruncated = "vk.ai.weaving.is_truncated";
    }
}
