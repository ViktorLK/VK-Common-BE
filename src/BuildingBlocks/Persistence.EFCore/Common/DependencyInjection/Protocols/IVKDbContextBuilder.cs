using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Defines a builder contract for configuring a specific DbContext.
/// Helps isolate options (such as database engines) per DbContext.
/// </summary>
/// <typeparam name="TContext">The concrete DbContext type.</typeparam>
public interface IVKDbContextBuilder<TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// Configures the DbContextOptions for this specific DbContext.
    /// </summary>
    /// <param name="configure">The configuration delegate.</param>
    void ConfigureOptions(Action<DbContextOptionsBuilder, IServiceProvider> configure);
}
