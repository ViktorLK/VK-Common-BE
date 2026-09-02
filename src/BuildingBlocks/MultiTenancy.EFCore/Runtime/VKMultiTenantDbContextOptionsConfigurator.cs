using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.MultiTenancy;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.MultiTenancy.EFCore;

/// <summary>
/// Configures DbContext options based on the current tenant's information.
/// Supports dynamic connection strings for database-level isolation.
/// </summary>
public sealed class VKMultiTenantDbContextOptionsConfigurator(
    IServiceProvider serviceProvider) : IVKDbContextOptionsConfigurator
{
    public void Configure(DbContextOptionsBuilder builder, IServiceProvider _)
    {
        var tenantContext = serviceProvider.GetService<IVKTenantContext>();
        if (tenantContext is null || !tenantContext.IsResolved)
        {
            return;
        }

        // Dynamic Connection String Isolation (Database-per-tenant)
        if (tenantContext.ConnectionString is not null && !tenantContext.ConnectionString.Value.IsEmpty)
        {
            ApplyConnectionString(builder, tenantContext.ConnectionString.Value.Reveal());
        }
    }

    private static void ApplyConnectionString(DbContextOptionsBuilder builder, string connectionString)
    {
        var extension = builder.Options.FindExtension<Microsoft.EntityFrameworkCore.Infrastructure.RelationalOptionsExtension>()
                        ?? throw new InvalidOperationException("No relational database provider has been configured for this DbContext. Dynamic connection string switching requires a relational provider.");

        extension = extension.WithConnectionString(connectionString);
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);
    }
}
