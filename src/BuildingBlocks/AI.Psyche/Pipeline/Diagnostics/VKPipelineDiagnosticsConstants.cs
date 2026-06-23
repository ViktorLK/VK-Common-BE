using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for Psyche execution and the pipeline.
/// </summary>
public static class VKPipelineDiagnosticsConstants
{
    public static class Logs
    {
        public const int ExecutionStarted = VKDiagnosticOffsets.AI_Psyche_Behaviors + 1;
        public const int ExecutionCompleted = VKDiagnosticOffsets.AI_Psyche_Behaviors + 2;
        public const int ExecutionFailed = VKDiagnosticOffsets.AI_Psyche_Behaviors + 3;

        public const int PipelineStarted = VKDiagnosticOffsets.AI_Psyche_Behaviors + 11;
        public const int PipelineCompleted = VKDiagnosticOffsets.AI_Psyche_Behaviors + 12;
        public const int PipelineFailed = VKDiagnosticOffsets.AI_Psyche_Behaviors + 13;
    }

    public static class Metrics
    {
        public const string ExecutionDuration = "vk.ai.psyche.pipeline.execution.duration";
        public const string PipelineDuration = "vk.ai.psyche.pipeline.duration";
    }
}
