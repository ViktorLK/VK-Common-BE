using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.AI.VectorStore.Sqlite.Common.DependencyInjection.Internal;

namespace VK.Blocks.AI.VectorStore.Sqlite;

/// <summary>
/// Fluent extensions for adding SQLite support to the AI Vector Store.
/// Following the Level 1 Public API pattern (AP.03).
/// </summary>
[ExcludeFromCodeCoverage]
public static class VKAIVectorStoreSqliteExtensions
{
    /// <summary>
    /// Adds the AI Vector Store SQLite implementation block to the service collection.
    /// Following the AddVK naming pattern (BB.03).
    /// </summary>
    public static IVKAIVectorStoreBuilder AddVKAIVectorStoreSqliteBlock(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddVKAIVectorStoreBlock(configuration)
                       .AddVKAISqliteDatabase();
    }

    /// <summary>
    /// Adds the AI Vector Store SQLite implementation block to the service collection with custom configuration.
    /// Following the AddVK naming pattern (BB.03).
    /// </summary>
    public static IVKAIVectorStoreBuilder AddVKAIVectorStoreSqliteBlock(
        this IServiceCollection services,
        Func<VKAIVectorStoreSqliteOptions, VKAIVectorStoreSqliteOptions> transform)
    {
        // For code-based registration, we use null for configuration as defaults are handled by the options record
        return services.AddVKAIVectorStoreBlock(null!)
                       .AddVKAISqliteDatabase(transform);
    }

    /// <summary>
    /// Adds the SQLite vector database implementation.
    /// Following the AddVK naming pattern (BB.03).
    /// </summary>
    public static IVKAIVectorStoreBuilder AddVKAISqliteDatabase(this IVKAIVectorStoreBuilder builder)
    {
        return AIVectorStoreSqliteRegistration.Register(builder, null);
    }

    /// <summary>
    /// Adds the SQLite vector database implementation with custom configuration.
    /// Following the AddVK naming pattern (BB.03).
    /// </summary>
    public static IVKAIVectorStoreBuilder AddVKAISqliteDatabase(
        this IVKAIVectorStoreBuilder builder,
        Func<VKAIVectorStoreSqliteOptions, VKAIVectorStoreSqliteOptions> transform)
    {
        return AIVectorStoreSqliteRegistration.Register(builder, transform);
    }
}
