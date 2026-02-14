using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.EFCore.Extensions;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Base class for Entity Framework Core DbContexts with common configuration.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDbContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
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

        modelBuilder.ApplyGlobalFilters();
        modelBuilder.ApplyConcurrencyToken();
    }

    #endregion
}
