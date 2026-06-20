using Microsoft.EntityFrameworkCore;
using VK.Blocks.MultiTenancy;
using VK.Blocks.Persistence.EFCore.Storage;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Base class for VKEntity Framework Core DbContexts with common configuration.
/// </summary>
public abstract class VKBaseDbContext : DbContext
{
    /// <summary>
    /// The current tenant identifier retrieved from the provider.
    /// </summary>
    public string? CurrentTenantId { get; }

    /// <summary>
    /// Evaluated by EF Core Global Query Filters during query execution. Throws if the tenant is missing.
    /// </summary>
    public string CurrentTenantIdForQueryFilter => CurrentTenantId 
        ?? throw new System.InvalidOperationException("Cannot query IVKMultiTenant entity: TenantId is missing from context. If you intend to query across all tenants, use .IgnoreQueryFilters().");

    /// <summary>
    /// Indicates whether MultiTenancy is enabled for this DB Context.
    /// </summary>
    public bool IsMultiTenancyEnabled { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKBaseDbContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="tenantProvider">The tenant provider to resolve the current tenant context.</param>
    /// <param name="persistenceOptions">The EF Core options containing feature flags.</param>
    protected VKBaseDbContext(DbContextOptions options, IVKTenantProvider? tenantProvider = null, VKPersistenceEFCoreOptions? persistenceOptions = null) : base(options)
    {
        CurrentTenantId = tenantProvider?.GetCurrentTenantId();
        IsMultiTenancyEnabled = persistenceOptions?.EnableMultiTenancy ?? false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKBaseDbContext"/> class.
    /// </summary>
    protected VKBaseDbContext()
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyGlobalFilters(this);
        modelBuilder.ApplyConcurrencyToken();
    }
}
