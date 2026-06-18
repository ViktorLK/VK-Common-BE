using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.Persistence.Sqlite.DependencyInjection.Internal;

namespace VK.Blocks.Persistence.Sqlite;

/// <summary>
/// Extension methods for setting up SQLite persistence services in an <see cref="IServiceCollection" />.
/// </summary>
public static class VKPersistenceSqliteBlockExtensions
{
    /// <summary>
    /// Adds and configures the Persistence SQLite block using <see cref="IConfiguration"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="dbContextOptions">Action to configure DbContext options.</param>
    /// <returns>The SQLite block builder.</returns>
    public static IVKPersistenceSqliteBuilder AddPersistenceSqliteBlock<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder, VKPersistenceSqliteOptions>? dbContextOptions = null)
        where TContext : DbContext
    {
        return SqliteBlockRegistration.Register<TContext>(services, configuration, null, dbContextOptions);
    }

    /// <summary>
    /// Adds and configures the Persistence SQLite block using a functional transform.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="transform">Function to transform options.</param>
    /// <param name="dbContextOptions">Action to configure DbContext options.</param>
    /// <returns>The SQLite block builder.</returns>
    public static IVKPersistenceSqliteBuilder AddPersistenceSqliteBlock<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<VKPersistenceSqliteOptions, VKPersistenceSqliteOptions> transform,
        Action<DbContextOptionsBuilder, VKPersistenceSqliteOptions>? dbContextOptions = null)
        where TContext : DbContext
    {
        return SqliteBlockRegistration.Register<TContext>(services, configuration, transform, dbContextOptions);
    }
}
