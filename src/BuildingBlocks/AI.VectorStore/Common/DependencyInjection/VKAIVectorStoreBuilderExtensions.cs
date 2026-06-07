using VK.Blocks.AI.VectorStore.Retrieval.Internal;
using VK.Blocks.AI.VectorStore.VectorStore.Internal;
using VK.Blocks.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.VectorStore.Retrieval.Protocols;
using VK.Blocks.AI.VectorStore.VectorStore.Protocols;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Fluent extensions for <see cref="IVKAIVectorStoreBuilder"/>.
/// </summary>
public static class VKAIVectorStoreBuilderExtensions
{
    /// <summary>
    /// Adds the In-Memory vector database implementation.
    /// </summary>
    public static IVKAIVectorStoreBuilder AddInMemoryDatabase(this IVKAIVectorStoreBuilder builder)
    {
        builder.WithScoped<VKAIVectorStoreBlock, IVKAIVectorStore, AIVectorStoreInMemoryDatabase>();
        return builder;
    }

    /// <summary>
    /// Adds a custom vector database implementation.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    public static IVKAIVectorStoreBuilder AddDatabaseProvider<TImplementation>(this IVKAIVectorStoreBuilder builder)
        where TImplementation : class, IVKAIVectorStore
    {
        builder.WithScoped<VKAIVectorStoreBlock, IVKAIVectorStore, TImplementation>();
        return builder;
    }

    /// <summary>
    /// Adds high-level Retrieval features (Chunks, Loaders, RAG Bridge) to the vector store.
    /// </summary>
    public static IVKAIVectorStoreBuilder AddRetrievalEngine(this IVKAIVectorStoreBuilder builder)
    {
        VKGuard.NotNull(builder);
        builder.Services.TryAddScoped<IVKRetrievalStore, VKVectorStoreRagEngine>();
        builder.Services.TryAddSingleton<IVKDocumentLoader, VKDocumentLoader>();
        return builder;
    }
}
