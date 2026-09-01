using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostic tokens for the Pattern feature.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static diagnostics and telemetry constants.")]
public static class VKPatternDiagnosticsConstants
{
    // Logs (Event IDs: 71000 - 71999)
    public static class Logs
    {
        public const int PatternInitialized = VKDiagnosticOffsets.AI_Psyche_Pattern + 1;
        public const int PatternResolved = VKDiagnosticOffsets.AI_Psyche_Pattern + 2;
    }

    public static class Metrics
    {
        public const string PatternStageDuration = "vk.ai.psyche.pattern.duration";
        public const string PatternsResolvedCount = "vk.ai.psyche.pattern.resolved_count";
    }

    public static class Tags
    {
        public const string StageName = "ai.psyche.stage";
        public const string PatternId = "ai.psyche.pattern.id";
    }
}
