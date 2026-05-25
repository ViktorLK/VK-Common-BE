using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Presence feature.
/// </summary>
public static class VKPresenceDiagnosticTokens
{
    // Logs (Event IDs)
    public const int PresenceInitializedEventId = VKDiagnosticOffsets.AI_Cognitive_Presence + 1;
    public const int QuotaExceededEventId = VKDiagnosticOffsets.AI_Cognitive_Presence + 2;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string ContributionQuotaUsage = "vk.ai.cognitive.presence.quota_usage";
        public const string StateSyncDuration = "vk.ai.cognitive.presence.state_sync_duration";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string TenantId = "vk.ai.presence.tenant_id";
        public const string AuditorAction = "vk.ai.presence.auditor_action";
    }
}
