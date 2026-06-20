using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.Persistence.Sqlite.DependencyInjection.Internal;

internal sealed class SqliteDbContextOptionsConfigurator(IOptions<VKPersistenceSqliteOptions> options) 
    : IVKDbContextOptionsConfigurator
{
    public void Configure(DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
    {
        var sqliteOptions = options.Value;
        if (sqliteOptions.Enabled && !string.IsNullOrWhiteSpace(sqliteOptions.ConnectionString))
        {
            builder.UseSqlite(sqliteOptions.ConnectionString);
        }
    }
}
