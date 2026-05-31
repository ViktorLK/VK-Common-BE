using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Cognitive Framing feature.
/// Follows OR.01.
/// </summary>
public static class VKFramingDiagnosticTokens
{
    // Logs (Event IDs)
    public const int FramingPipelineStartedEventId = VKDiagnosticOffsets.AI_Cognitive_Framing + 1;
    public const int FramingPipelineCompletedEventId = VKDiagnosticOffsets.AI_Cognitive_Framing + 2;
    public const int FramingFallbackTriggeredEventId = VKDiagnosticOffsets.AI_Cognitive_Framing + 3;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string FramingDuration = "vk.ai.cognitive.framing.duration";
        public const string ReservedSystemTokens = "vk.ai.cognitive.framing.reserved_system_tokens";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string TenantId = "vk.ai.cognitive.tenant_id";
        public const string UserId = "vk.ai.cognitive.user_id";
        public const string TelemetryStressState = "vk.ai.cognitive.stress_state";
    }
}
