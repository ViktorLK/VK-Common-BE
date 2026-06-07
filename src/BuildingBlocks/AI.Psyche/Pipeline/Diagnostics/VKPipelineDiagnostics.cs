using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostics constants for the Orchestration Pipeline.
/// </summary>
public static class VKPipelineDiagnostics
{
    public const int PipelineStartedEventId = VKDiagnosticOffsets.AI_Afferent_Orchestration + 1;
    public const int PipelineCompletedEventId = VKDiagnosticOffsets.AI_Afferent_Orchestration + 2;
    public const int PipelineFailedEventId = VKDiagnosticOffsets.AI_Afferent_Orchestration + 3;

    public static class Metrics
    {
        public const string PipelineDuration = "vk.ai.psyche.pipeline.duration";
    }
}
