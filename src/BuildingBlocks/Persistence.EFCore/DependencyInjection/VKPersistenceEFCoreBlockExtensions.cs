using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.DependencyInjection.Internal;
using VK.Blocks.Persistence.EFCore.Pagination.Internal;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Extension methods for setting up persistence services in an <see cref="IServiceCollection" />.
/// </summary>
public static class VKPersistenceEFCoreBlockExtensions
{
    /// <summary>
    /// Adds and configures the Persistence EF Core block using <see cref="IConfiguration"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="dbContextOptions">Action to configure DbContext options.</param>
    /// <returns>The EF Core block builder.</returns>
    public static IVKPersistenceEFCoreBuilder AddPersistenceEFCoreBlock<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder, VKPersistenceEFCoreOptions>? dbContextOptions = null)
        where TContext : DbContext
    {
        return PersistenceEFCoreBlockRegistration.Register<TContext>(services, configuration, null, dbContextOptions);
    }

    /// <summary>
    /// Adds and configures the Persistence EF Core block using a functional transform.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="transform">Function to transform options.</param>
    /// <param name="dbContextOptions">Action to configure DbContext options.</param>
    /// <returns>The EF Core block builder.</returns>
    public static IVKPersistenceEFCoreBuilder AddPersistenceEFCoreBlock<TContext>(
        this IServiceCollection services,
        Func<VKPersistenceEFCoreOptions, VKPersistenceEFCoreOptions> transform,
        Action<DbContextOptionsBuilder, VKPersistenceEFCoreOptions>? dbContextOptions = null)
        where TContext : DbContext
    {
        return PersistenceEFCoreBlockRegistration.Register<TContext>(services, null!, transform, dbContextOptions);
    }

    /// <summary>
    /// Overrides the default cursor serializer with SecureCursorSerializer.
    /// </summary>
    /// <param name="builder">The EF Core block builder.</param>
    /// <param name="transform">An optional function to transform VKCursorSerializerOptions.</param>
    /// <returns>The EF Core block builder.</returns>
    public static IVKPersistenceEFCoreBuilder AddSecureCursorSerializer(
        this IVKPersistenceEFCoreBuilder builder,
        Func<VKCursorSerializerOptions, VKCursorSerializerOptions>? transform = null)
    {
        // Replace the default SimpleCursorSerializer with the secure implementation.
        builder.Services.RemoveAll<IVKCursorSerializer>();
        builder.Services.TryAddSingleton<IVKCursorSerializer, SecureCursorSerializer>();

        // We register the options even without IConfiguration for manual overrides.
        builder.Services.AddVKBlockOptions(null!, transform);

        return builder;
    }
}
