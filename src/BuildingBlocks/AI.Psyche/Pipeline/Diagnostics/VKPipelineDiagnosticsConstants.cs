using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for the Psyche pipeline.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static diagnostics and telemetry constants.")]
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
        public const string PipelineExecutionDuration = "vk.ai.psyche.pipeline.execution.duration";
        public const string PipelineDuration = "vk.ai.psyche.pipeline.duration";
        public const string StageDuration = "vk.ai.psyche.pipeline.stage.duration";
        public const string LLMInvocationDuration = "vk.ai.psyche.pipeline.llm.duration";
    }

    public static class Tags
    {
        public const string StageName = "ai.psyche.stage";
        public const string IsSuccess = "ai.psyche.is_success";
        public const string ErrorCode = "error.code";
        public const string Model = "gen_ai.request.model";
    }
}
