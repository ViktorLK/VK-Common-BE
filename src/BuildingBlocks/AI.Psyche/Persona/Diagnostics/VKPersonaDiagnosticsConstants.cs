using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for the Persona feature.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static diagnostics and telemetry constants.")]
public static class VKPersonaDiagnosticsConstants
{
    public static class Logs
    {
        public const int PersonaResolved = VKDiagnosticOffsets.AI_Psyche_Persona + 1;
        public const int PersonaRendered = VKDiagnosticOffsets.AI_Psyche_Persona + 2;
    }

    public static class Metrics
    {
        public const string PersonaStageDuration = "vk.ai.psyche.persona.duration";
        public const string PersonasResolvedCount = "vk.ai.psyche.persona.resolved_count";
    }

    public static class Tags
    {
        public const string StageName = "ai.psyche.stage";
        public const string PersonaId = "ai.psyche.persona.id";
    }
}
