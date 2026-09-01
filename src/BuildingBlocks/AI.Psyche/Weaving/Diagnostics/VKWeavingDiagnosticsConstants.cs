using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for the Weaving Engine.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static diagnostics and telemetry constants.")]
public static class VKWeavingDiagnosticsConstants
{
    public static class Logs
    {
        public const int WeavingTruncated = VKDiagnosticOffsets.AI_Psyche_Weaving + 1;
        public const int WeavingAssembled = VKDiagnosticOffsets.AI_Psyche_Weaving + 2;
        public const int WeavingEmptyActive = VKDiagnosticOffsets.AI_Psyche_Weaving + 3;
    }

    public static class Metrics
    {
        public const string WeavingDuration = "vk.ai.psyche.weaving.duration";
        public const string TokensBudgetExceeded = "vk.ai.psyche.weaving.budget_exceeded";
        public const string TokensAssembled = "vk.ai.psyche.weaving.tokens_assembled";
    }

    public static class Tags
    {
        public const string StageName = "ai.psyche.stage";
        public const string MessageCount = "ai.psyche.weaving.message_count";
        public const string EvictedCount = "ai.psyche.weaving.evicted_count";
        public const string Budget = "ai.psyche.weaving.budget";
    }
}
