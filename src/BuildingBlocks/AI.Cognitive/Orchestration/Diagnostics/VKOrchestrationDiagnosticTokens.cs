using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Orchestration feature.
/// </summary>
public static class VKOrchestrationDiagnosticTokens
{
    // Logs (Event IDs)
    public const int IntentRoutingFailedEventId = VKDiagnosticOffsets.AI_Cognitive_Orchestration + 1;
    public const int PipelineStartedEventId = VKDiagnosticOffsets.AI_Cognitive_Orchestration + 2;
    public const int PipelineCompletedEventId = VKDiagnosticOffsets.AI_Cognitive_Orchestration + 3;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string PipelineExecutionDuration = "vk.ai.cognitive.orchestration.pipeline_duration";
        public const string IntentRoutingDuration = "vk.ai.cognitive.orchestration.routing_duration";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string PipelineStage = "vk.ai.orchestration.pipeline_stage";
        public const string Intent = "vk.ai.orchestration.intent";
    }
}
