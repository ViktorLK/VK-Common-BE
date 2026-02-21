using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.MultiTenancy.Exceptions;
using VK.Blocks.Persistence.EFCore.Interceptors;
using VK.Blocks.Core.Primitives;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Interceptors;

public class TenantInterceptorTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly TenantInterceptor _sut;

    public TenantInterceptorTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _tenantProviderMock = _fixture.Freeze<Mock<ITenantProvider>>();
        _sut = new TenantInterceptor(_tenantProviderMock.Object);
    }

    public class TestMultiTenantEntity : IMultiTenant
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestMultiTenantEntity> Entities { get; set; } = null!;
    }

    private TestDbContext CreateContext()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

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
