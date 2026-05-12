using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.VectorStore.Databases;
using VK.Blocks.AI.VectorStore.Databases.Internal;
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
        => AIVectorStoreInMemoryRegistration.Register(builder);

    /// <summary>
    /// Adds a custom vector database implementation.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
    public static IVKAIVectorStoreBuilder AddDatabaseProvider<TImplementation>(this IVKAIVectorStoreBuilder builder)
        where TImplementation : class, IVKAIVectorDatabase
    {
        builder.WithScoped<VKAIVectorStoreBlock, IVKAIVectorDatabase, TImplementation>();
        return builder;
    }
}
