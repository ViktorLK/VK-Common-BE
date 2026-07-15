using System;
using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Persistence.EFCore.Sqlite.Common.DependencyInjection.Internal;

internal sealed class SqliteDbContextOptionsConfigurator(VKPersistenceEFCoreSqliteOptions options, VKPersistenceEFCoreSqliteOptions defaultsOptions)
    : IVKDbContextOptionsConfigurator
{
    public void Configure(DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
    {
        if (options.Enabled && !string.IsNullOrWhiteSpace(defaultsOptions.ConnectionString))
        {
            builder.UseSqlite(defaultsOptions.ConnectionString);
        }
    }
}
