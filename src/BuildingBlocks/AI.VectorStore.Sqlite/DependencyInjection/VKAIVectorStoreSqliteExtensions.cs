using System;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.VectorStore.Sqlite;
using VK.Blocks.AI.VectorStore.Sqlite.DependencyInjection.Internal;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Fluent extensions for adding SQLite support to the AI Vector Store.
/// Following the Level 1 Public API pattern (AP.03).
/// </summary>
public static class VKAIVectorStoreSqliteExtensions
{
    /// <summary>
    /// Adds the SQLite vector database implementation.
    /// </summary>
    public static IVKAIVectorStoreBuilder AddSqliteDatabase(this IVKAIVectorStoreBuilder builder)
        => AIVectorStoreSqliteRegistration.Register(builder, null);

    /// <summary>
    /// Adds the SQLite vector database implementation with custom configuration.
    /// </summary>
    public static IVKAIVectorStoreBuilder AddSqliteDatabase(
        this IVKAIVectorStoreBuilder builder,
        Func<VKAIVectorStoreSqliteOptions, VKAIVectorStoreSqliteOptions> transform)
        => AIVectorStoreSqliteRegistration.Register(builder, transform);
}
