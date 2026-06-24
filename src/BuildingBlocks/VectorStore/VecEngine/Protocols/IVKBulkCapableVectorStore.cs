using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore; // [AP.03] public API surface in flat root namespace

/// <summary>
/// Optional capability interface for vector stores supporting optimized bulk upsert operations.
/// </summary>
public interface IVKBulkCapableVectorStore : IVKVectorStore
{
    /// <summary>
    /// Performs an optimized batch upsert across collections.
    /// </summary>
    Task<VKResult> UpsertBatchAsync<T>(
        string collectionName,
        IEnumerable<(string Id, T Document, VKVector Vector)> records,
        CancellationToken cancellationToken = default) where T : class;
}
