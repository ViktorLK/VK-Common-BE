using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.Persistence.Sqlite.DependencyInjection.Internal;

internal static class SqliteBlockRegistration
{
    public static IVKPersistenceSqliteBuilder Register<TContext>(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKPersistenceSqliteOptions, VKPersistenceSqliteOptions>? transformOptions = null,
        Action<DbContextOptionsBuilder, VKPersistenceSqliteOptions>? dbContextOptions = null)
        where TContext : DbContext
    {
        var builder = new PersistenceSqliteBuilder(services, configuration);

        // 1. Check-Self & Prerequisite (Standard BB.03 - Throws if parent EFCore is not registered on the outside)
        if (services.IsVKBlockRegistered<VKPersistenceSqliteBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        var options = services.AddVKBlockOptions<VKPersistenceSqliteOptions>(configuration, transformOptions);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKPersistenceSqliteBlock>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKPersistenceSqliteOptions>, SqliteOptionsValidator>();

        // 5. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Core Services / Register Sqlite DbContext Configurator (AP.02)
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKDbContextOptionsConfigurator, SqliteDbContextOptionsConfigurator>());

        return builder;
    }
}
