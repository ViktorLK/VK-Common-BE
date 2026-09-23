using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines a pipeline stage in Psyche that orchestrates jobs or tasks.
/// Inherits from <see cref="IVKPipelineStage{TContext}"/>.
/// Supports zero-reflection OpenTelemetry tracing via <see cref="TraceName"/> and <see cref="StageName"/>.
/// </summary>
public interface IVKPsychePipelineStage : IVKPipelineStage<VKPsycheContext>
{
    /// <summary>
    /// Gets the OpenTelemetry trace span activity name for this stage execution.
    /// Defaults to "psyche.stage.custom" for un-customized external stages.
    /// </summary>
    string TraceName => "psyche.stage.custom";

    /// <summary>
    /// Gets the normalized stage identifier token used for profiling and metrics.
    /// Defaults to "Custom".
    /// </summary>
    string StageName => "Custom";
}
