using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for Psyche execution behaviors and the pipeline.
/// </summary>
public static class VKBehaviorsDiagnostics
{
    public const int ExecutionStartedEventId = 10500 + 1;
    public const int ExecutionCompletedEventId = 10500 + 2;
    public const int ExecutionFailedEventId = 10500 + 3;

    public const int PipelineStartedEventId = VKDiagnosticOffsets.AI_Afferent_Orchestration + 1;
    public const int PipelineCompletedEventId = VKDiagnosticOffsets.AI_Afferent_Orchestration + 2;
    public const int PipelineFailedEventId = VKDiagnosticOffsets.AI_Afferent_Orchestration + 3;

    public static class Metrics
    {
        public const string ExecutionDuration = "vk.ai.psyche.pipeline.execution.duration";
        public const string PipelineDuration = "vk.ai.psyche.pipeline.duration";
    }
}
