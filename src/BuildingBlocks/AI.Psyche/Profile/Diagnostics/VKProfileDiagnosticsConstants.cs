using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostic tokens for the Profile feature.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static diagnostics and telemetry constants.")]
public static class VKProfileDiagnosticsConstants
{
    // Logs (Event IDs: 77000 - 77999)
    public static class Logs
    {
        public const int ProfileInitialized = VKDiagnosticOffsets.AI_Psyche_Profile + 1;
        public const int ProfileResolved = VKDiagnosticOffsets.AI_Psyche_Profile + 2;
    }

    public static class Metrics
    {
        public const string ProfileStageDuration = "vk.ai.psyche.profile.duration";
        public const string ProfilesResolvedCount = "vk.ai.psyche.profile.resolved_count";
    }

    public static class Tags
    {
        public const string StageName = "ai.psyche.stage";
        public const string ProfileId = "ai.psyche.profile.id";
    }
}
