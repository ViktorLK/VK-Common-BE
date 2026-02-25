using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.Persistence.EFCore.Tests;

/// <summary>
/// A test <see cref="BaseDbContext"/> implementation for unit and integration tests.
/// </summary>
public class TestDbContext : BaseDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the products.
    /// </summary>
    public DbSet<TestProduct> Products { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });
    }
}
