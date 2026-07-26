using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines a pipeline stage in Psyche that orchestrates jobs or tasks.
/// Inherits from <see cref="IVKPipelineStage{TContext}"/>.
/// </summary>
public interface IVKPsychePipelineStage : IVKPipelineStage<VKPsycheContext>;
