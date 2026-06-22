using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Optional capability interface for vector stores that support native server-side hybrid search.
/// </summary>
public interface IVKHybridCapableVectorStore : IVKVectorStore
{
    /// <summary>
    /// Performs a native hybrid search (dense vector + sparse text search) on the specified collection.
    /// </summary>
    Task<VKResult<IEnumerable<VKVectorRecord<T>>>> SearchHybridAsync<T>(
        string collectionName,
        VKVector vector,
        string searchText,
        VKVectorSearchArgs args,
        CancellationToken cancellationToken = default) where T : class;
}
