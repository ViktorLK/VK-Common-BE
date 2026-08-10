using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines an atomic task in VectorSearch.
/// Inherits from <see cref="IVKPipelineTask{TContext, TResult}"/>.
/// </summary>
public interface IVKVectorSearchPipelineTask<TResult> : IVKPipelineTask<VKVectorSearchContext, TResult>;
