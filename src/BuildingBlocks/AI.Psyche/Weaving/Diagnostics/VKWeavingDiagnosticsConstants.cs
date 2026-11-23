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
        public const int WeavingTruncated = VKPsycheDiagnosticOffsets.Weaving + 1;
        public const int WeavingAssembled = VKPsycheDiagnosticOffsets.Weaving + 2;
        public const int WeavingEmptyActive = VKPsycheDiagnosticOffsets.Weaving + 3;
        public const int ContextBudgetExceeded = VKPsycheDiagnosticOffsets.Weaving + 4;
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
