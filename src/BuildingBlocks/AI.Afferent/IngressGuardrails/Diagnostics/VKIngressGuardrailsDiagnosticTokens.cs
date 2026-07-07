using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Afferent;

public static class VKIngressGuardrailsDiagnosticTokens
{
    public const int GuardrailsPipelineStartedEventId = VKDiagnosticOffsets.AI_Guardrails + 1;
    public const int GuardrailsPipelineCompletedEventId = VKDiagnosticOffsets.AI_Guardrails + 2;
    public const int GuardrailsViolationDetectedEventId = VKDiagnosticOffsets.AI_Guardrails + 3;
}
