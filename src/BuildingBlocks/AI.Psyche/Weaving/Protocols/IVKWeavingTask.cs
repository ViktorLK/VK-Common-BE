using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents an individual task in the prompt weaving pipeline.
/// Inherits from <see cref="IVKPipelineTask{TContext}"/>.
/// </summary>
public interface IVKWeavingTask : IVKPipelineTask<VKPsycheContext>;
