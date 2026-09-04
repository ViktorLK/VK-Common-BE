using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Database.Internal;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Base class for VKEntity Framework Core DbContexts with common configuration.
/// </summary>
public abstract class VKBaseDbContext : DbContext
{
    /// <summary>
    private readonly IVKTenantProvider? _tenantProvider;

    /// <summary>
    /// The current tenant identifier retrieved from the provider.
    /// </summary>
    public VKTenantId? CurrentTenantId => _tenantProvider?.GetCurrentTenantId();

    /// <summary>
    /// Evaluated by EF Core Global Query Filters during query execution.
    /// Returns the current tenant identifier or <c>null</c> if unassigned.
    /// <para>
    /// <b>Defense-in-Depth Note:</b> When <see cref="IsMultiTenancyEnabled"/> is <c>true</c> and <see cref="CurrentTenantId"/> is <c>null</c>,
    /// EF Core queries on <see cref="IVKTenantScoped"/> entities will evaluate <c>e.TenantId is null</c> resulting in 0 rows returned.
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

    private readonly VKPersistenceEFCoreOptions _options;
    private readonly IEnumerable<IVKModelCreatingContributor> _creatingContributors;
    private readonly IEnumerable<IVKModelConventionContributor> _conventionContributors;
    private readonly IEnumerable<IVKGlobalFilterContributor> _filterContributors;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKBaseDbContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="tenantProvider">The tenant provider to resolve the current tenant context.</param>
    /// <param name="defaultOptions">The EF Core options containing feature flags.</param>
    /// <param name="creatingContributors">Optional collection of model creating contributors.</param>
    /// <param name="conventionContributors">Optional collection of convention contributors.</param>
    /// <param name="filterContributors">Optional collection of global filter contributors.</param>
    protected VKBaseDbContext(
        DbContextOptions options,
        IVKTenantProvider? tenantProvider = null,
        VKPersistenceEFCoreOptions? defaultOptions = null,
        IEnumerable<IVKModelCreatingContributor>? creatingContributors = null,
        IEnumerable<IVKModelConventionContributor>? conventionContributors = null,
        IEnumerable<IVKGlobalFilterContributor>? filterContributors = null) : base(options)
    {
        _tenantProvider = tenantProvider;
        IsMultiTenancyEnabled = tenantProvider is not null;
        _options = defaultOptions ?? new VKPersistenceEFCoreOptions();

        _creatingContributors = creatingContributors ?? [];
        _conventionContributors = conventionContributors ?? [];
        _filterContributors = filterContributors ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKBaseDbContext"/> class.
    /// </summary>
    protected VKBaseDbContext()
    {
        _options = new VKPersistenceEFCoreOptions();
        _creatingContributors = [];
        _conventionContributors = [];
        _filterContributors = [];
    }

    /// <inheritdoc />
    protected sealed override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<VKTenantId>().HaveConversion<VKTenantIdConverter>();
        configurationBuilder.Properties<VKUserId>().HaveConversion<VKUserIdConverter>();

        // 1. Execute all registered model convention contributors
        foreach (var contributor in _conventionContributors)
        {
            contributor.ConfigureConventions(configurationBuilder);
        }

        // 2. Hook for derived DbContext custom conventions
        ConfigureConventionsCustom(configurationBuilder);
    }

    /// <summary>
    /// Custom convention configuration hook for derived DbContexts.
    /// </summary>
    /// <param name="configurationBuilder">The builder being used to configure conventions for this context.</param>
    protected virtual void ConfigureConventionsCustom(ModelConfigurationBuilder configurationBuilder)
    {
    }

    /// <inheritdoc />
    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Hook for derived DbContexts and Building Blocks to register entity models and configurations
        OnModelCreatingCustom(modelBuilder);

        // 2. Execute all registered model creating contributors (e.g., ConcurrencyModelContributor)
        foreach (var contributor in _creatingContributors)
        {
            contributor.ConfigureModel(modelBuilder);
        }

        // 3. Finalize standardized column ordering across ALL registered entities (CS.08)
        if (_options.EnableColumnOrdering)
        {
            // [CS.08]
            modelBuilder.ApplyColumnOrdering();
        }

        // 4. Execute all dynamic global query filter contributors (soft delete, multi-tenant, row-level security)
        foreach (var contributor in _filterContributors)
        {
            contributor.ApplyFilter(modelBuilder, this);
        }
    }

    /// <summary>
    /// Custom model configuration hook for derived DbContexts to apply entity configurations and conventions.
    /// Executed before base multi-tenancy, concurrency, and global query filters are configured.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected virtual void OnModelCreatingCustom(ModelBuilder modelBuilder)
    {
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
