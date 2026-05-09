using System;
using Microsoft.EntityFrameworkCore;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Defines a contract for configuring <see cref="DbContextOptions"/> at runtime.
/// This allows for dynamic configuration (e.g., connection strings, provider-specific settings) 
/// based on the current scope and services.
/// </summary>
public interface IVKDbContextOptionsConfigurator
{
    /// <summary>
    /// Configures the <see cref="DbContextOptionsBuilder"/> using the provided scoped service provider.
    /// </summary>
    /// <param name="builder">The builder used to configure the context options.</param>
    /// <param name="serviceProvider">The scoped service provider for the current request.</param>
    void Configure(DbContextOptionsBuilder builder, IServiceProvider serviceProvider);
}
