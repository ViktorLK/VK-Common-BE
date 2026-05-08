using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.MultiTenancy;

namespace VK.Blocks.Persistence.EFCore.MultiTenancy.Internal;

/// <summary>
/// Configures DbContext options based on the current tenant's information.
/// Supports dynamic connection strings for database-level isolation.
/// </summary>
internal sealed class MultiTenantDbContextOptionsConfigurator(
    IOptions<VKPersistenceEFCoreOptions> options,
    IServiceProvider serviceProvider) : IVKDbContextOptionsConfigurator
{
    private readonly VKPersistenceEFCoreOptions _options = options.Value;

    public void Configure(DbContextOptionsBuilder builder, IServiceProvider _)
    {
        if (_options.EnableMultiTenancy != true)
        {
            return;
        }

        // We resolve IVKTenantContext from the internal service provider to access current tenant info.
        var tenantContext = serviceProvider.GetService<IVKTenantContext>();
        var currentTenant = tenantContext?.CurrentTenant;

        if (currentTenant is null)
        {
            return;
        }

        // 1. Dynamic Connection String Isolation
        if (!string.IsNullOrWhiteSpace(currentTenant.ConnectionString))
        {
            // Note: We expect the database provider (UseSqlServer, UseNpgsql, etc.)
            // to have been pre-configured in AddVKDbContext.
            // We only override the connection string here.
            // This requires the specific provider's Extension to be able to handle subsequent UseXxx calls.
            // A more robust way is to use a custom IConnectionFactory, but overriding ConnectionString is a common first step.

            // For most providers, calling UseXxx again with a different connection string will override the previous one.
            // However, since we don't know the provider here, we rely on the fact that 'builder.Options'
            // already contains the provider info.

            // Implementation Detail: This configurator is meant to be called AFTER the initial provider setup.
            ApplyConnectionString(builder, currentTenant.ConnectionString);
        }
    }

    private static void ApplyConnectionString(DbContextOptionsBuilder builder, string connectionString)
    {
        // EF Core models its relational configuration via RelationalOptionsExtension.
        // By modifying this extension, we can inject a new connection string that will be used
        // regardless of whether the provider is SQL Server, PostgreSQL, or MySQL.

        var extension = builder.Options.FindExtension<Microsoft.EntityFrameworkCore.Infrastructure.RelationalOptionsExtension>()
                        ?? throw new InvalidOperationException("No relational database provider has been configured for this DbContext. Dynamic connection string switching requires a relational provider.");

        // Create a new extension with the updated connection string.
        extension = extension.WithConnectionString(connectionString);

        // Re-apply the extension to the builder.
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);
    }
}
