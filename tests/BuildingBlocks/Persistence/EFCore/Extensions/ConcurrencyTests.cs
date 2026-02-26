using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Core.Primitives;
using VK.Blocks.Persistence.EFCore.Extensions;
using VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Extensions;

/// <summary>
/// A test entity that implements <see cref="IConcurrency"/>.
/// </summary>
public class ConcurrencyEntity : IConcurrency
{
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public int Id { get; set; }

    /// <inheritdoc />
    public byte[] RowVersion { get; set; } = [];
}

/// <summary>
/// A test database context for concurrency tests.
/// </summary>
public class ConcurrencyContext : BaseDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public ConcurrencyContext(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Gets or sets the concurrency entities.
    /// </summary>
    public DbSet<ConcurrencyEntity> ConcurrencyEntities { get; set; } = null!;
}

/// <summary>
/// Integration tests for concurrency token application.
/// </summary>
public class ConcurrencyTests : IntegrationTestBase<ConcurrencyContext>
{
    /// <summary>
    /// Verifies that the concurrency token (RowVersion) is correctly configured in the model.
    /// </summary>
    [Fact]
    public void ApplyConcurrencyToken_SetsRowVersion()
    {
        // Act
        // Context is already created and EnsureCreatedAsync called by base class.
        var configuredEntity = Context.Model.FindEntityType(typeof(ConcurrencyEntity));
        var property = configuredEntity!.FindProperty(nameof(IConcurrency.RowVersion));

        // Assert
        property.Should().NotBeNull();
        property!.IsConcurrencyToken.Should().BeTrue();
    }
}
