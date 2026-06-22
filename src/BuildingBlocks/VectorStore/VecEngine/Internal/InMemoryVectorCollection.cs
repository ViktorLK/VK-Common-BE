using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.VecEngine.Internal;

/// <summary>
/// Basic in-memory implementation of a vector collection.
/// Following AP.03 Naming Taxonomy.
/// </summary>
internal sealed class InMemoryVectorCollection<T>(
    string name,
    InMemoryVectorStore database) : IVKVectorCollection<T> where T : class
{
    public string Name => name;

    public Task<VKResult> UpsertAsync(
        string id,
        T document,
        VKVector vector,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(database.UpsertGeneric(Name, id, document, vector));
    }

    public Task<VKResult<IEnumerable<VKVectorRecord<T>>>> SearchAsync(
        VKVector vector,
        VKVectorSearchArgs args,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(database.SearchGeneric<T>(Name, vector, args));
    }

    public Task<VKResult> DeleteAsync(string id, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(database.DeleteGeneric(Name, id));
    }

    public async Task<VKResult> UpsertBatchAsync(
        IEnumerable<(string Id, T Document, VKVector Vector)> records,
        CancellationToken cancellationToken = default)
    {
        foreach (var record in records)
        {
            var result = await UpsertAsync(record.Id, record.Document, record.Vector, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
                return result;
        }
        return VKResult.Success();
    }

    public async IAsyncEnumerable<VKResult<VKVectorRecord<T>>> SearchStreamAsync(
        VKVector vector,
        VKVectorSearchArgs args,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = await SearchAsync(vector, args, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            yield return VKResult.Failure<VKVectorRecord<T>>(result.Errors);
            yield break;
        }

        foreach (var record in result.Value)
        {
            yield return VKResult.Success(record);
        }
    }

    public Task<VKResult<VKVectorRecord<T>?>> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // [CS.01] Return Result pattern, no null references
        return Task.FromResult(database.GetByIdGeneric<T>(Name, id));
    }
}
