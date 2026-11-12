using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKGuardrailsDiagnosticsConstants
{
    public const int ContentFlaggedEventId = VKAIDiagnosticOffsets.Guardrails + 1;
    public const int ModerationCheckPerformedEventId = VKAIDiagnosticOffsets.Guardrails + 2;
    public const int ModerationRequestFailedEventId = VKAIDiagnosticOffsets.Guardrails + 3;
}
