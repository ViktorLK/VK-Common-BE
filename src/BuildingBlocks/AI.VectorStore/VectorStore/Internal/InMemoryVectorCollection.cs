using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.AI.VectorStore.VectorStore.Protocols;

namespace VK.Blocks.AI.VectorStore.VectorStore.Internal;

/// <summary>
/// In-memory implementation of a vector collection.
/// </summary>
internal sealed class InMemoryVectorCollection<T>(
    string name,
    AIVectorStoreInMemoryDatabase database) : IVKAIVectorCollection<T> where T : class
{
    public string Name => name;

    public Task<VKResult> UpsertAsync(
        string id,
        T document,
        VKEmbeddingsVector vector,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(database.UpsertGeneric(Name, id, document, vector));
    }

    public Task<VKResult<IEnumerable<VKAIVectorRecord<T>>>> SearchAsync(
        VKEmbeddingsVector vector,
        VKAIVectorSearchArgs args,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(database.SearchGeneric<T>(Name, vector, args));
    }

    public Task<VKResult> DeleteAsync(string id, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(database.DeleteGeneric(Name, id));
    }

    public async Task<VKResult> UpsertBatchAsync(
        IEnumerable<(string Id, T Document, VKEmbeddingsVector Vector)> records,
        CancellationToken cancellationToken = default)
    {
        foreach (var record in records)
        {
            var result = await UpsertAsync(record.Id, record.Document, record.Vector, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure) return result;
        }
        return VKResult.Success();
    }

    public async IAsyncEnumerable<VKResult<VKAIVectorRecord<T>>> SearchStreamAsync(
        VKEmbeddingsVector vector,
        VKAIVectorSearchArgs args,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = await SearchAsync(vector, args, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            yield return VKResult.Failure<VKAIVectorRecord<T>>(result.Errors);
            yield break;
        }

        foreach (var record in result.Value)
        {
            yield return VKResult.Success(record);
        }
    }
}
