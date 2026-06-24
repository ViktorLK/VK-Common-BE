using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Entry point for executing the Vector Search pipeline chain.
/// </summary>
public interface IVKVectorSearchPipelineExecutor : IVKPipelineExecutor<VKVectorSearchContext, VKSearchResult[]> {}
