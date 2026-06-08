using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Public diagnostic tokens for the Afferent Guardrails feature.
/// Follows OR.01.
/// </summary>
public static class VKAfferentGuardrailsDiagnosticTokens
{
    // Logs (Event IDs)
    public const int GuardrailsPipelineStartedEventId = VKDiagnosticOffsets.AI_Afferent_Guardrails + 1;
    public const int GuardrailsPipelineCompletedEventId = VKDiagnosticOffsets.AI_Afferent_Guardrails + 2;
    public const int GuardrailsViolationDetectedEventId = VKDiagnosticOffsets.AI_Afferent_Guardrails + 3;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string GuardrailsDuration = "vk.ai.Afferent.guardrails.duration";
        public const string GuardrailsViolationsCount = "vk.ai.Afferent.guardrails.violations";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string TenantId = "vk.ai.Afferent.tenant_id";
        public const string UserId = "vk.ai.Afferent.user_id";
        public const string ViolationType = "vk.ai.Afferent.violation_type";
    }
}
