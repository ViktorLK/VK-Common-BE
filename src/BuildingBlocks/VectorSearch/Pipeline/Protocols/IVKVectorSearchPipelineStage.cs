using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines a pipeline stage in VectorSearch that orchestrates jobs or tasks.
/// Inherits from <see cref="IVKPipelineStage{TContext}"/>.
/// </summary>
public interface IVKVectorSearchPipelineStage : IVKPipelineStage<VKVectorSearchContext>;
