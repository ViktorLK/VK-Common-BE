using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Reasoning feature.
/// </summary>
public static class VKReasoningDiagnosticTokens
{
    // Logs (Event IDs)
    public const int GoalDecompositionFailedEventId = VKDiagnosticOffsets.AI_Cognitive_Reasoning + 1;
    public const int StepParsingFailedEventId = VKDiagnosticOffsets.AI_Cognitive_Reasoning + 2;
    public const int PlanningCompletedEventId = VKDiagnosticOffsets.AI_Cognitive_Reasoning + 3;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string DecompositionDepth = "vk.ai.cognitive.reasoning.decomposition_depth";
        public const string PlanningDuration = "vk.ai.cognitive.reasoning.planning_duration";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string GoalId = "vk.ai.reasoning.goal_id";
        public const string StepStatus = "vk.ai.reasoning.step_status";
    }
}
