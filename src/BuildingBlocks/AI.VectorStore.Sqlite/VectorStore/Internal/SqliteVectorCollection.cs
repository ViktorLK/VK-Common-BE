using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.VectorStore.Contracts;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Sqlite.VectorStore.Internal;

/// <summary>
/// SQLite implementation of a vector collection.
/// </summary>
internal sealed class SqliteVectorCollection<T>(
    string name,
    AIVectorStoreSqliteDatabase database) : IVKAIVectorCollection<T> where T : class
{
    public string Name => name;

    public async Task<VKResult> UpsertAsync(
        string id,
        T document,
        VKEmbeddingVector vector,
        CancellationToken cancellationToken = default)
    {
        return await database.UpsertGenericAsync(Name, id, document, vector, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult<IEnumerable<VKAIVectorRecord<T>>>> SearchAsync(
        VKEmbeddingVector vector,
        VKAIVectorSearchArgs args,
        CancellationToken cancellationToken = default)
    {
        return await database.SearchGenericAsync<T>(Name, vector, args, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> DeleteAsync(string id, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        return await database.DeleteGenericAsync(Name, id, tenantId ?? string.Empty, cancellationToken).ConfigureAwait(false);
    }
}
