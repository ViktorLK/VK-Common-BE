using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Sqlite.SqliteVec.Internal;

/// <summary>
/// SQLite implementation of a vector collection.
/// Following AP.03 Naming Taxonomy.
/// </summary>
internal sealed class SqliteVectorCollection<T>(
    string name,
    SqliteVectorStore database) : IVKVectorCollection<T> where T : class
{
    public string Name => name;

    public async Task<VKResult> UpsertAsync(
        string id,
        T document,
        VKVector vector,
        CancellationToken cancellationToken = default)
    {
        return await database.UpsertGenericAsync(Name, id, document, vector, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult<IEnumerable<VKVectorRecord<T>>>> SearchAsync(
        VKVector vector,
        VKVectorSearchArgs args,
        CancellationToken cancellationToken = default)
    {
        return await database.SearchGenericAsync<T>(Name, vector, args, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> DeleteAsync(string id, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        return await database.DeleteGenericAsync(Name, id, tenantId ?? string.Empty, cancellationToken).ConfigureAwait(false);
    }

    public Task<VKResult> UpsertBatchAsync(IEnumerable<(string Id, T Document, VKVector Vector)> records, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public IAsyncEnumerable<VKResult<VKVectorRecord<T>>> SearchStreamAsync(VKVector vector, VKVectorSearchArgs args, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public async Task<VKResult<VKVectorRecord<T>?>> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // [CS.03] ConfigureAwait(false) on all awaits in libraries
        return await database.GetByIdGenericAsync<T>(Name, id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult<bool>> ExistsAsync(VKMetadataFilter filter, CancellationToken cancellationToken = default)
    {
        // [CS.03] ConfigureAwait(false)
        return await database.ExistsGenericAsync(Name, filter, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult<IEnumerable<VKVectorRecord<T>>>> QueryAsync(VKMetadataFilter filter, CancellationToken cancellationToken = default)
    {
        // [CS.03] ConfigureAwait(false)
        return await database.QueryGenericAsync<T>(Name, filter, cancellationToken).ConfigureAwait(false);
    }
}
