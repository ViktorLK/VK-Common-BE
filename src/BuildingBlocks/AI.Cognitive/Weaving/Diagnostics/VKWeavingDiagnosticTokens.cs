namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Weaving feature.
/// </summary>
public static class VKWeavingDiagnosticTokens
{
    // Logs (Event IDs)
    public const int WeavingInitializedEventId = 100;
    public const int TokenLimitWarningEventId = 101;
    public const int TapestryWeavedEventId = 102;

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
