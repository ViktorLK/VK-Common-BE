using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.VectorStore.Sqlite.Common.DependencyInjection.Internal;

namespace VK.Blocks.VectorStore.Sqlite;

/// <summary>
/// Fluent extensions for adding SQLite support to the Vector Store.
/// Following the Level 1 Public API pattern (AP.03).
/// </summary>
[ExcludeFromCodeCoverage]
public static class VKVectorStoreSqliteBlockExtensions
{
    /// <summary>
    /// Adds the Vector Store SQLite implementation block to the service collection.
    /// Following the AddVK naming pattern (BB.03).
    /// </summary>
    public static IVKVectorStoreBuilder AddVKVectorStoreSqliteBlock(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddVKVectorStoreBlock(configuration)
                   .AddVKVectorStoreSqlite();

    /// <summary>
    /// Adds the Vector Store SQLite implementation block to the service collection with custom configuration.
    /// Following the AddVK naming pattern (BB.03).
    /// </summary>
    public static IVKVectorStoreBuilder AddVKVectorStoreSqliteBlock(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKVectorStoreSqliteOptions, VKVectorStoreSqliteOptions> transform)
        => services.AddVKVectorStoreBlock(configuration)
                   .AddVKVectorStoreSqlite(transform);

    /// <summary>
    /// Adds the SQLite vector database implementation.
    /// Following the AddVK naming pattern (BB.03).
    /// </summary>
    public static IVKVectorStoreBuilder AddVKVectorStoreSqlite(this IVKVectorStoreBuilder builder)
        => VectorStoreSqliteBlockRegistration.Register(builder, null);

    /// <summary>
    /// Adds the SQLite vector database implementation with custom configuration.
    /// Following the AddVK naming pattern (BB.03).
    /// </summary>
    public static IVKVectorStoreBuilder AddVKVectorStoreSqlite(
        this IVKVectorStoreBuilder builder,
        Func<VKVectorStoreSqliteOptions, VKVectorStoreSqliteOptions> transform)
        => VectorStoreSqliteBlockRegistration.Register(builder, transform);
}
