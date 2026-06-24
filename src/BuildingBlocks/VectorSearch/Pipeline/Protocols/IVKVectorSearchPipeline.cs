using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// The vector search execution pipeline. Executes registered search stages.
/// </summary>
public interface IVKVectorSearchPipeline : IVKPipeline<VKSearchQuery, VKSearchResult[]> {}
