using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.EFCore;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests;

/// <summary>
/// Integration tests for <see cref="BaseDbContext"/>.
/// </summary>
public class BaseDbContextTests
{
    /// <summary>
    /// A test implementation of <see cref="BaseDbContext"/>.
    /// </summary>
    private class TestDbContext : BaseDbContext
    {
        public TestDbContext(DbContextOptions options) : base(options) { }
        public TestDbContext() : base() { }
    }

    /// <summary>
    /// Verifies that the parameterless constructor can be called and initializes default values.
    /// </summary>
    [Fact]
    public void ParameterlessConstructor_CanBeCalled()
    {
        // Act
        var sut = new TestDbContext();

        // Assert
        sut.Should().NotBeNull();
        sut.CurrentTenantId.Should().BeNull();
        sut.IsMultiTenancyEnabled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the constructor with options initializes correctly.
    /// </summary>
    [Fact]
    public void Constructor_WithOptions_InitializesCorrectly()
    {
        // Arrange
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        // Act
        var sut = new TestDbContext(options);

        // Assert
        sut.Should().NotBeNull();
        sut.CurrentTenantId.Should().BeNull();
        sut.IsMultiTenancyEnabled.Should().BeFalse();
    }
}
