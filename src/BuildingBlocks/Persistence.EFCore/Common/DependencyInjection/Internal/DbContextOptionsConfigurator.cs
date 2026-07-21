using System;
using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Persistence.EFCore.Common.DependencyInjection.Internal;

internal sealed class DbContextOptionsConfigurator<TContext>(
    Action<DbContextOptionsBuilder, IServiceProvider> configure) : IVKDbContextOptionsConfigurator
    where TContext : DbContext
{
    public void Configure(DbContextOptionsBuilder builder, IServiceProvider sp)
    {
        if (builder is DbContextOptionsBuilder<TContext>)
        {
            configure(builder, sp);
        }
    }
}
