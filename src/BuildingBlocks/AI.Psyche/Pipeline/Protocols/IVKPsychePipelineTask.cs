using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines an atomic task in Psyche that performs specific prompt manipulation/processing.
/// Inherits from <see cref="IVKPipelineTask{TContext, TResult}"/>.
/// </summary>
public interface IVKPsychePipelineTask<TResult> : IVKPipelineTask<VKPsycheContext, TResult>;
