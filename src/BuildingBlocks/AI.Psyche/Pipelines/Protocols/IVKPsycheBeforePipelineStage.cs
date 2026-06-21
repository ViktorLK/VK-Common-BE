using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines a pipeline stage that runs BEFORE the LLM call in Psyche.
/// Follows CS.01, CS.03.
/// </summary>
public interface IVKPsycheBeforePipelineStage : IVKBeforePipelineStage<VKPsycheContext> {}
