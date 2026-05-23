namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Reasoning feature.
/// </summary>
public static class VKReasoningDiagnosticTokens
{
    // Logs (Event IDs)
    public const int GoalDecompositionFailedEventId = 700;
    public const int StepParsingFailedEventId = 701;
    public const int PlanningCompletedEventId = 702;

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
