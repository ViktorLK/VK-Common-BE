using VK.Blocks.VectorStore.VecEngine.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Fluent extensions for <see cref="IVKVectorStoreBuilder"/>.
/// </summary>
public static class VKVectorStoreBuilderExtensions
{
    /// <summary>
    /// Adds the In-Memory vector database implementation.
    /// </summary>
    public static IVKVectorStoreBuilder AddInMemoryDatabase(this IVKVectorStoreBuilder builder)
    {
        builder.WithScoped<VKVectorStoreBlock, IVKVectorStore, InMemoryVectorStore>();
        return builder;
    }

    /// <summary>
    /// Adds a custom vector database implementation.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    public static IVKVectorStoreBuilder AddDatabaseProvider<TImplementation>(this IVKVectorStoreBuilder builder)
        where TImplementation : class, IVKVectorStore
    {
        builder.WithScoped<VKVectorStoreBlock, IVKVectorStore, TImplementation>();
        return builder;
    }
}
