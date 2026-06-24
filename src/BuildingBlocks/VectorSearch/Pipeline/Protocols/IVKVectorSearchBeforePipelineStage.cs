using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines a pipeline stage that runs BEFORE the search terminal action in VectorSearch.
/// </summary>
public interface IVKVectorSearchBeforePipelineStage : IVKBeforePipelineStage<VKVectorSearchContext> {}
