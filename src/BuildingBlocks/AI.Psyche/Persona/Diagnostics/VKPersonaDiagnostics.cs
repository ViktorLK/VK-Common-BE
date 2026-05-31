using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for the Persona feature.
/// </summary>
public static class VKPersonaDiagnostics
{
    public const int PersonaResolvedEventId = VKDiagnosticOffsets.AI_Cognitive_Persona + 1;
    public const int PersonaRenderedEventId = VKDiagnosticOffsets.AI_Cognitive_Persona + 2;
}
