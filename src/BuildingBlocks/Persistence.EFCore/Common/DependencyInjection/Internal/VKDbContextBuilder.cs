using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Persistence.EFCore.Common.DependencyInjection.Internal;

internal sealed class VKDbContextBuilder<TContext>(
    IServiceCollection services,
    IConfiguration configuration) : IVKDbContextBuilder<TContext>
    where TContext : DbContext
{
    public IServiceCollection Services { get; } = services;
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureOptions(Action<DbContextOptionsBuilder, IServiceProvider> configure)
    {
        Services.AddTransient<IVKDbContextOptionsConfigurator>(sp => 
            new DbContextOptionsConfigurator<TContext>(configure));
    }
}

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
