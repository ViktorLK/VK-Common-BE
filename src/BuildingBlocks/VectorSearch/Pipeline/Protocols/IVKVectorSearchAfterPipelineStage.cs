using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines a pipeline stage that runs AFTER the search terminal action in VectorSearch.
/// </summary>
public interface IVKVectorSearchAfterPipelineStage : IVKAfterPipelineStage<VKVectorSearchContext> {}
