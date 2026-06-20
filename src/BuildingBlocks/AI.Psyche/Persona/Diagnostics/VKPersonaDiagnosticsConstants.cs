using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for the Persona feature.
/// </summary>
public static class VKPersonaDiagnosticsConstants
{
    public static class Logs
    {
        public const int PersonaResolved = VKDiagnosticOffsets.AI_Psyche_Persona + 1;
        public const int PersonaRendered = VKDiagnosticOffsets.AI_Psyche_Persona + 2;
    }
}
