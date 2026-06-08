using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Public diagnostic tokens for the Afferent Text feature.
/// Follows OR.01.
/// </summary>
public static class VKAfferentTextDiagnosticTokens
{
    // Logs (Event IDs)
    public const int TextPipelineStartedEventId = VKDiagnosticOffsets.AI_Afferent_Text + 1;
    public const int TextPipelineCompletedEventId = VKDiagnosticOffsets.AI_Afferent_Text + 2;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string TextDuration = "vk.ai.Afferent.text.duration";
        public const string TextProcessedLength = "vk.ai.Afferent.text.processed_length";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string TenantId = "vk.ai.Afferent.tenant_id";
        public const string UserId = "vk.ai.Afferent.user_id";
    }
}
