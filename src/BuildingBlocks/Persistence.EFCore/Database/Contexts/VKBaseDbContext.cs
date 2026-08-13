using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VK.Blocks.Core;
using VK.Blocks.MultiTenancy;
using VK.Blocks.Persistence.EFCore.Database.Internal;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Base class for VKEntity Framework Core DbContexts with common configuration.
/// </summary>
public abstract class VKBaseDbContext : DbContext
{
    /// <summary>
    /// The current tenant identifier retrieved from the provider.
    /// </summary>
    public VKTenantId? CurrentTenantId { get; }

    /// <summary>
    /// Evaluated by EF Core Global Query Filters during query execution.
    /// Returns the current tenant identifier or <c>null</c> if unassigned.
    /// <para>
    /// <b>Defense-in-Depth Note:</b> When <see cref="IsMultiTenancyEnabled"/> is <c>true</c> and <see cref="CurrentTenantId"/> is <c>null</c>,
    /// EF Core queries on <see cref="IVKMultiTenant"/> entities will evaluate <c>e.TenantId == null</c> resulting in 0 rows returned.
    /// This DB-level zero-row degradation is a Fail-Closed defense-in-depth safety net to prevent cross-tenant data leakage.
    /// It is <b>NOT</b> the primary detection point for tenant resolution failures. Upstream ASP.NET Core middleware 
    /// or authorization handlers MUST intercept and reject unauthenticated/unresolved tenant requests before reaching the persistence layer.
    /// </para>
    /// </summary>
    public VKTenantId? CurrentTenantIdForQueryFilter => CurrentTenantId;

    /// <summary>
    /// Indicates whether MultiTenancy is enabled for this DB Context.
    /// </summary>
    public bool IsMultiTenancyEnabled { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKBaseDbContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="tenantProvider">The tenant provider to resolve the current tenant context.</param>
    /// <param name="defaultOptions">The EF Core options containing feature flags.</param>
    protected VKBaseDbContext(DbContextOptions options, IVKTenantProvider? tenantProvider = null, VKPersistenceEFCoreOptions? defaultOptions = null) : base(options)
    {
        CurrentTenantId = tenantProvider?.GetCurrentTenantId();
        IsMultiTenancyEnabled = defaultOptions?.EnableMultiTenancy ?? false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKBaseDbContext"/> class.
    /// </summary>
    protected VKBaseDbContext()
    {
    }

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<VKTenantId>().HaveConversion<VKTenantIdConverter>();
        configurationBuilder.Properties<VKUserId>().HaveConversion<VKUserIdConverter>();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyGlobalFilters(this);
        modelBuilder.ApplyConcurrencyToken();
    }

    private sealed class VKTenantIdConverter : ValueConverter<VKTenantId, Guid>
    {
        public VKTenantIdConverter() : base(
            id => id.Value,
            value => new VKTenantId(value))
        {
        }
    }

    private sealed class VKUserIdConverter : ValueConverter<VKUserId, Guid>
    {
        public VKUserIdConverter() : base(
            id => id.Value,
            value => new VKUserId(value))
        {
        }
    }
}
