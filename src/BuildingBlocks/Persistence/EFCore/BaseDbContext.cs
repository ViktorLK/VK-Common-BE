using Microsoft.EntityFrameworkCore;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.Persistence.Abstractions.Options;
using VK.Blocks.Persistence.EFCore.Extensions;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Base class for Entity Framework Core DbContexts with common configuration.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    /// <summary>
    /// The current tenant identifier retrieved from the provider.
    /// </summary>
    public string? CurrentTenantId { get; }

    /// <summary>
    /// Indicates whether MultiTenancy is enabled for this DB Context.
    /// </summary>
    public bool IsMultiTenancyEnabled { get; }

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDbContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="tenantProvider">The tenant provider to resolve the current tenant context.</param>
    /// <param name="persistenceOptions">The persistence options containing feature flags.</param>
    protected BaseDbContext(DbContextOptions options, ITenantProvider? tenantProvider = null, PersistenceOptions? persistenceOptions = null) : base(options)
    {
        CurrentTenantId = tenantProvider?.GetCurrentTenantId();
        IsMultiTenancyEnabled = persistenceOptions?.EnableMultiTenancy ?? false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
    /// </summary>
    protected BaseDbContext()
    {
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyGlobalFilters(this);
        modelBuilder.ApplyConcurrencyToken();
    }

    #endregion
}
