using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines a middleware that runs in the VectorSearch pipeline.
/// </summary>
public interface IVKVectorSearchMiddleware : IVKMiddleware<VKVectorSearchContext, VKSearchResult[]> {}
