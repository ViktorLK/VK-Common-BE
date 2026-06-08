using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Public diagnostic tokens for the Afferent Tokenics feature.
/// Follows OR.01.
/// </summary>
public static class VKAfferentTokenicsDiagnosticTokens
{
    // Logs (Event IDs)
    public const int TokenicsPipelineStartedEventId = VKDiagnosticOffsets.AI_Afferent_Tokenics + 1;
    public const int TokenicsPipelineCompletedEventId = VKDiagnosticOffsets.AI_Afferent_Tokenics + 2;
    public const int TokenicsBudgetExceededEventId = VKDiagnosticOffsets.AI_Afferent_Tokenics + 3;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string TokenicsDuration = "vk.ai.Afferent.tokenics.duration";
        public const string InputTokensCount = "vk.ai.Afferent.tokenics.input_tokens";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string TenantId = "vk.ai.Afferent.tenant_id";
        public const string UserId = "vk.ai.Afferent.user_id";
    }
}
