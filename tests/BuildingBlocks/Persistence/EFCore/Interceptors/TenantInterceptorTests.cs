using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using VK.Blocks.Core.Primitives;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.MultiTenancy.Exceptions;
using VK.Blocks.Persistence.EFCore.Interceptors;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Interceptors;

/// <summary>
/// Unit tests for <see cref="TenantInterceptor"/>.
/// </summary>
public class TenantInterceptorTests
{
    private readonly IFixture _fixture;

    private readonly Mock<ITenantProvider> _tenantProviderMock;

    private readonly TenantInterceptor _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantInterceptorTests"/> class.
    /// </summary>
    public TenantInterceptorTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _tenantProviderMock = _fixture.Freeze<Mock<ITenantProvider>>();
        _sut = new TenantInterceptor(_tenantProviderMock.Object);
    }

    /// <summary>
    /// A test entity that implements <see cref="IMultiTenant"/>.
    /// </summary>
    public class TestMultiTenantEntity : IMultiTenant
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public int Id { get; set; }

        /// <inheritdoc />
        public string TenantId { get; set; } = string.Empty;
    }

    /// <summary>
    /// A test database context for multi-tenancy tests.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TestDbContext"/> class.
    /// </remarks>
    /// <param name="options">The database context options.</param>
    private class TestDbContext(DbContextOptions<TenantInterceptorTests.TestDbContext> options) : DbContext(options)
    {

        /// <summary>
        /// Gets or sets the multi-tenant entities.
        /// </summary>
        public DbSet<TestMultiTenantEntity> Entities { get; set; } = null!;
    }

    /// <summary>
    /// Helper method to create a database context with SQLite in-memory.
    /// </summary>
    private static TestDbContext CreateContext()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new TestDbContext(options);

        // Rationale: Ensure the schema is created before running any operations.
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Verifies that <see cref="TenantInterceptor.SavingChanges"/> injects the current TenantId when it's missing.
    /// </summary>
    [Fact]
    public void SavingChanges_NewEntityWithoutTenantId_InjectsTenantId()
    {
        // Arrange
        using var context = CreateContext();
        var entity = new TestMultiTenantEntity { Id = 1 };
        context.Entities.Add(entity);
        var tenantId = "tenant-1";
        _tenantProviderMock.Setup(x => x.GetCurrentTenantId()).Returns(tenantId);

        var eventData = new DbContextEventData(null!, null!, context);

        // Act
        _sut.SavingChanges(eventData, new InterceptionResult<int>());

        // Assert
        entity.TenantId.Should().Be(tenantId);
    }

    /// <summary>
    /// Verifies that <see cref="TenantInterceptor.SavingChangesAsync"/> injects the current TenantId when it's missing.
    /// </summary>
    [Fact]
    public async Task SavingChangesAsync_NewEntityWithoutTenantId_InjectsTenantId()
    {
        // Arrange
        using var context = CreateContext();
        var entity = new TestMultiTenantEntity { Id = 1 };
        context.Entities.Add(entity);
        var tenantId = "tenant-1";
        _tenantProviderMock.Setup(x => x.GetCurrentTenantId()).Returns(tenantId);

        var eventData = new DbContextEventData(null!, null!, context);

        // Act
        await _sut.SavingChangesAsync(eventData, new InterceptionResult<int>());

        // Assert
        entity.TenantId.Should().Be(tenantId);
    }

    /// <summary>
    /// Verifies that <see cref="TenantInterceptor.SavingChanges"/> does not overwrite an already set TenantId.
    /// </summary>
    [Fact]
    public void SavingChanges_TenantIdAlreadySet_DoesNotOverwrite()
    {
        // Arrange
        using var context = CreateContext();
        var existingTenantId = "already-set";
        var entity = new TestMultiTenantEntity { Id = 1, TenantId = existingTenantId };
        context.Entities.Add(entity);
        _tenantProviderMock.Setup(x => x.GetCurrentTenantId()).Returns("new-tenant");

        var eventData = new DbContextEventData(null!, null!, context);

        // Act
        _sut.SavingChanges(eventData, new InterceptionResult<int>());

        // Assert
        entity.TenantId.Should().Be(existingTenantId);
    }

    /// <summary>
    /// Verifies that <see cref="TenantInterceptor.SavingChanges"/> throws <see cref="TenantNotProvidedException"/> when the tenant is missing from the provider.
    /// </summary>
    [Fact]
    public void SavingChanges_TenantMissingFromProvider_ThrowsTenantNotProvidedException()
    {
        // Arrange
        using var context = CreateContext();
        var entity = new TestMultiTenantEntity { Id = 1 };
        context.Entities.Add(entity);
        _tenantProviderMock.Setup(x => x.GetCurrentTenantId()).Returns((string?)null);

        var eventData = new DbContextEventData(null!, null!, context);

        // Act
        Action act = () => _sut.SavingChanges(eventData, new InterceptionResult<int>());

        // Assert
        act.Should().Throw<TenantNotProvidedException>();
    }

    /// <summary>
    /// Verifies that <see cref="TenantInterceptor.SavingChanges"/> returns a valid result even when the context is null.
    /// </summary>
    [Fact]
    public void SavingChanges_NullContext_ReturnsResult()
    {
        // Arrange
        var eventData = new DbContextEventData(null!, null!, null);

        // Act
        var result = _sut.SavingChanges(eventData, new InterceptionResult<int>());

        // Assert
        result.Should().NotBeNull();
    }
}
