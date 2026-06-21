using VK.Blocks.AI.VectorStore.VecEngine.Internal;
using VK.Blocks.Core;

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
        builder.WithScoped<VKAIVectorStoreBlock, IVKAIVectorStore, BasicAIVectorStore>();
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
}
