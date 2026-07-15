using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore;
using VK.Blocks.Persistence.EFCore.Sqlite;

namespace VK.Blocks.Persistence.EFCore.Sqlite;

/// <summary>
/// Extension methods for configuring SQLite database provider on a DbContext builder.
/// </summary>
public static class VKPersistenceEFCoreSqliteBuilderExtensions
{
    /// <summary>
    /// Configures SQLite as the database provider for this DbContext.
    /// </summary>
    /// <typeparam name="TContext">The concrete DbContext type.</typeparam>
    /// <param name="builder">The DbContext builder.</param>
    /// <param name="configure">Optional options transform delegate.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IVKDbContextBuilder<TContext> UseSqlite<TContext>(
        this IVKDbContextBuilder<TContext> builder,
        Func<VKPersistenceEFCoreSqliteOptions, VKPersistenceEFCoreSqliteOptions>? configure = null)
        where TContext : DbContext
    {
        VKGuard.NotNull(builder);

        // 1. Register the SQLite Block services (TryAdd ensures idempotency)
        _ = builder.Services.AddVKPersistenceEFCoreSqliteBlock(builder.Configuration, configure);

        // 2. Configure SQLite on this DbContext's options builder
        builder.ConfigureOptions((dbBuilder, sp) =>
        {
            var options = sp.GetRequiredService<IOptions<VKPersistenceEFCoreSqliteOptions>>().Value;
            dbBuilder.UseSqlite(options.ConnectionString);
        });

        return builder;
    }
}
