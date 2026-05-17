using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.AI.VectorStore.Contracts;
using VK.Blocks.Core;

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
}
