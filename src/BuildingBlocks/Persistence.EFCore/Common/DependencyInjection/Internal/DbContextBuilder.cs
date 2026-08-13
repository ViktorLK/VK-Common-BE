using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Persistence.EFCore.Common.DependencyInjection.Internal;

internal sealed class DbContextBuilder<TContext>(
    IServiceCollection services,
    IConfiguration configuration) : IVKDbContextBuilder<TContext>
    where TContext : DbContext
{
    public IServiceCollection Services { get; } = services;
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureOptions(Action<DbContextOptionsBuilder, IServiceProvider> configure)
    {
        // [AP.02 Waiver]: Multiple options configurators are intentionally accumulated for execution via GetServices<IVKDbContextOptionsConfigurator>().
        Services.AddTransient<IVKDbContextOptionsConfigurator>(sp => 
            new DbContextOptionsConfigurator<TContext>(configure));
    }
}
