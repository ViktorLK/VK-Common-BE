using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostic tokens for the Session feature.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static diagnostics and telemetry constants.")]
public static class VKSessionDiagnosticsConstants
{
    // Logs (Event IDs: 78000 - 78999)
    public static class Logs
    {
        public const int SessionInitialized = VKPsycheDiagnosticOffsets.Session + 1;
        public const int SessionResolved = VKPsycheDiagnosticOffsets.Session + 2;
        public const int SessionUpdated = VKPsycheDiagnosticOffsets.Session + 3;
        public const int SessionNotActive = VKPsycheDiagnosticOffsets.Session + 4;
    }

    public static class Metrics
    {
        public const string SessionResolveDuration = "vk.ai.psyche.session.resolve.duration";
        public const string SessionUpdateDuration = "vk.ai.psyche.session.update.duration";
        public const string ActiveSessionsResolvedCount = "vk.ai.psyche.session.resolved_count";
        public const string SessionTurnCount = "vk.ai.psyche.session.turn_count";
    }

    public static class Tags
    {
        public const string StageName = "ai.psyche.stage";
        public const string SessionId = "gen_ai.session.id";
        public const string Status = "ai.psyche.session.status";
    }
}
