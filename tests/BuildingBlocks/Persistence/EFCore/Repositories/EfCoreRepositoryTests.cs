using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Core.Primitives;
using VK.Blocks.Persistence.Core.Pagination;
using VK.Blocks.Core.Results;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.EFCore.Repositories;
using VK.Blocks.Persistence.EFCore.Services;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Repositories;

public class EfCoreRepositoryTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly Mock<IAuditProvider> _auditProviderMock;
    private readonly Mock<ILogger<EfCoreRepository<TestEntity>>> _loggerMock;
    private readonly Mock<IEntityLifecycleProcessor> _lifecycleProcessorMock;
    private readonly Mock<ICursorSerializer> _cursorSerializerMock;
    private readonly List<SqliteConnection> _connections = new();

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class AuditableSoftDeleteEntity : IAuditable, ISoftDelete
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // IAuditable
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // ISoftDelete
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }

    private class TestsDbContext : DbContext
    {
        public TestsDbContext(DbContextOptions<TestsDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;
        public DbSet<AuditableSoftDeleteEntity> AuditableSoftDeleteEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedNever();
            });
            modelBuilder.Entity<AuditableSoftDeleteEntity>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedNever();
            });
        }
    }

    public EfCoreRepositoryTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _auditProviderMock = _fixture.Freeze<Mock<IAuditProvider>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<EfCoreRepository<TestEntity>>>>();
        _lifecycleProcessorMock = _fixture.Freeze<Mock<IEntityLifecycleProcessor>>();
        _cursorSerializerMock = _fixture.Freeze<Mock<ICursorSerializer>>();
    }

    private (EfCoreRepository<TestEntity> sut, TestsDbContext context) CreateSutWithSqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        _connections.Add(connection);

        var options = new DbContextOptionsBuilder<TestsDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new TestsDbContext(options);
        context.Database.EnsureCreated();

        var sut = new EfCoreRepository<TestEntity>(
            context,
            _loggerMock.Object,
            _cursorSerializerMock.Object,
            _lifecycleProcessorMock.Object);

        return (sut, context);
    }

    private (EfCoreRepository<AuditableSoftDeleteEntity> sut, TestsDbContext context) CreateAuditableSutWithSqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        _connections.Add(connection);

        var options = new DbContextOptionsBuilder<TestsDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;
        var context = new TestsDbContext(options);
        context.Database.EnsureCreated();

        var sut = new EfCoreRepository<AuditableSoftDeleteEntity>(
            context,
            Mock.Of<ILogger<EfCoreRepository<AuditableSoftDeleteEntity>>>(),
            _cursorSerializerMock.Object,
            _lifecycleProcessorMock.Object);

        return (sut, context);
    }

    public void Dispose()
    {
        foreach (var connection in _connections)
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task AddAsync_ValidEntity_ShouldAddEntity()
    {
        // Arrange
        var entity = _fixture.Create<TestEntity>();
        var (sut, context) = CreateSutWithSqlite();

        // Act
        var result = await sut.AddAsync(entity);
        await context.SaveChangesAsync();

        // Assert
        result.Should().BeEquivalentTo(entity);
        context.TestEntities.Should().ContainEquivalentOf(entity);
    }

    [Fact]
    public async Task UpdateAsync_ValidEntity_ShouldUpdateEntity()
    {
        // Arrange
        var entity = _fixture.Create<TestEntity>();
        var (sut, context) = CreateSutWithSqlite();

        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        entity.Name = "Updated Name";
        await sut.UpdateAsync(entity);
        await context.SaveChangesAsync();

        // Assert
        var updatedEntity = await context.TestEntities.FindAsync(entity.Id);
        updatedEntity!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_ValidEntity_ShouldRemoveEntity()
    {
        // Arrange
        var entity = _fixture.Create<TestEntity>();
        var (sut, context) = CreateSutWithSqlite();

        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        await sut.DeleteAsync(entity);
        await context.SaveChangesAsync();

        // Assert
        var deletedEntity = await context.TestEntities.FindAsync(entity.Id);
        deletedEntity.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteUpdateAsync_ConditionMet_UpdatesEntities()
    {
        // Arrange
        var entity1 = _fixture.Build<TestEntity>().With(x => x.Id, 1).With(x => x.Value, 10).Create();
        var entity2 = _fixture.Build<TestEntity>().With(x => x.Id, 2).With(x => x.Value, 20).Create();
        var (sut, context) = CreateSutWithSqlite();

        context.TestEntities.AddRange(entity1, entity2);
        await context.SaveChangesAsync();

        // Act
        var count = await sut.ExecuteUpdateAsync(
            e => e.Id == entity1.Id,
            setter => setter.SetProperty(e => e.Value, 999)
        );

        // Assert
        count.Should().Be(1);
        context.ChangeTracker.Clear();
        var updatedEntity = await context.TestEntities.FindAsync(entity1.Id);
        updatedEntity!.Value.Should().Be(999);

        var otherEntity = await context.TestEntities.FindAsync(entity2.Id);
        otherEntity!.Value.Should().Be(20);
    }

    [Fact]
    public async Task ExecuteUpdateAsync_AuditableEntity_UpdatesAuditFields()
    {
        // Arrange
        var entity = new AuditableSoftDeleteEntity { Id = 1, Name = "Original" };
        var (sut, context) = CreateAuditableSutWithSqlite();

        context.AuditableSoftDeleteEntities.Add(entity);
        await context.SaveChangesAsync();

        var utcNow = DateTime.UtcNow;
        var userId = "test_user";

        // Setup processor to simulate auditing logic
        _lifecycleProcessorMock.Setup(p => p.ProcessBulkUpdate(It.IsAny<IPropertySetter<AuditableSoftDeleteEntity>>()))
            .Callback<IPropertySetter<AuditableSoftDeleteEntity>>(setter =>
            {
                setter.SetProperty(e => ((IAuditable)e).UpdatedAt, utcNow);
                setter.SetProperty(e => ((IAuditable)e).UpdatedBy, userId);
            });

        // Act
        await sut.ExecuteUpdateAsync(
            e => e.Id == 1,
            setter => setter.SetProperty(e => e.Name, "Updated"));

        // Assert
        context.ChangeTracker.Clear();
        var updated = await context.AuditableSoftDeleteEntities.FindAsync(1);

        updated!.Name.Should().Be("Updated");
        updated.UpdatedAt.Should().Be(utcNow);
        updated.UpdatedBy.Should().Be(userId);

        _lifecycleProcessorMock.Verify(p => p.ProcessBulkUpdate(It.IsAny<IPropertySetter<AuditableSoftDeleteEntity>>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteDeleteAsync_ConditionMet_DeletesEntities()
    {
        // Arrange
        var entity1 = _fixture.Build<TestEntity>().With(x => x.Id, 1).With(x => x.Value, 10).Create();
        var entity2 = _fixture.Build<TestEntity>().With(x => x.Id, 2).With(x => x.Value, 20).Create();
        var (sut, context) = CreateSutWithSqlite();
        context.TestEntities.AddRange(entity1, entity2);
        await context.SaveChangesAsync();

        // Act
        var count = await sut.ExecuteDeleteAsync(e => e.Value == 10);

        // Assert
        count.Should().Be(1);
        context.ChangeTracker.Clear();
        context.TestEntities.Count().Should().Be(1);
        context.TestEntities.FirstOrDefault()!.Id.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteDeleteAsync_SoftDeleteEntity_PerformsSoftDelete()
    {
        // Arrange
        var entity = new AuditableSoftDeleteEntity { Id = 1, Name = "Original" };
        var (sut, context) = CreateAuditableSutWithSqlite();

        context.AuditableSoftDeleteEntities.Add(entity);
        await context.SaveChangesAsync();

        var utcNow = DateTime.UtcNow;

        // Setup processor to simulate soft delete logic
        _lifecycleProcessorMock.Setup(p => p.ProcessBulkSoftDelete(It.IsAny<IPropertySetter<AuditableSoftDeleteEntity>>()))
            .Callback<IPropertySetter<AuditableSoftDeleteEntity>>(setter =>
            {
                setter.SetProperty(e => ((ISoftDelete)e).IsDeleted, true);
                setter.SetProperty(e => ((ISoftDelete)e).DeletedAt, utcNow);
            });

        // Act
        var count = await sut.ExecuteDeleteAsync(e => e.Id == 1);

        // Assert
        count.Should().Be(1); // 1 row affected
        context.ChangeTracker.Clear();

        // Should still exist in DB but marked deleted
        var softDeleted = await context.AuditableSoftDeleteEntities.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == 1);
        softDeleted.Should().NotBeNull();
        softDeleted!.IsDeleted.Should().BeTrue();
        softDeleted.DeletedAt.Should().Be(utcNow);

        _lifecycleProcessorMock.Verify(p => p.ProcessBulkSoftDelete(It.IsAny<IPropertySetter<AuditableSoftDeleteEntity>>()), Times.Once);
    }

    [Fact]
    public async Task AddRangeAsync_ValidEntities_ShouldAddEntities()
    {
        // Arrange
        var entities = _fixture.CreateMany<TestEntity>(3).ToList();
        var (sut, context) = CreateSutWithSqlite();

        // Act
        await sut.AddRangeAsync(entities);
        await context.SaveChangesAsync();

        // Assert
        context.TestEntities.Should().HaveCount(3);
        context.TestEntities.Should().ContainEquivalentOf(entities[0]);
    }

    [Fact]
    public async Task UpdateRangeAsync_ValidEntities_ShouldUpdateEntities()
    {
        // Arrange
        var entities = _fixture.CreateMany<TestEntity>(3).ToList();
        var (sut, context) = CreateSutWithSqlite();
        context.TestEntities.AddRange(entities);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        foreach (var e in entities)
            e.Name += "_Updated";
        await sut.UpdateRangeAsync(entities);
        await context.SaveChangesAsync();

        // Assert
        var dbEntities = await context.TestEntities.ToListAsync();
        dbEntities.Should().AllSatisfy(e => e.Name.Should().EndWith("_Updated"));
    }

    [Fact]
    public async Task DeleteRangeAsync_ValidEntities_ShouldDeleteEntities()
    {
        // Arrange
        var entities = _fixture.CreateMany<TestEntity>(3).ToList();
        var (sut, context) = CreateSutWithSqlite();
        context.TestEntities.AddRange(entities);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        await sut.DeleteRangeAsync(entities);
        await context.SaveChangesAsync();

        // Assert
        context.TestEntities.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        Action act = () => new EfCoreRepository<TestEntity>(null!, _loggerMock.Object, _cursorSerializerMock.Object, _lifecycleProcessorMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
