using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VK.Blocks.Core.Primitives;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.Core.Pagination;
using VK.Blocks.Persistence.EFCore.Infrastructure;
using VK.Blocks.Persistence.EFCore.Repositories;
using VK.Blocks.Persistence.EFCore.Services;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Repositories;

/// <summary>
/// Unit tests for <see cref="EfCoreRepository{TEntity}"/>.
/// </summary>
public class EfCoreRepositoryTests : IDisposable
{
    private readonly IFixture _fixture;

    private readonly Mock<IAuditProvider> _auditProviderMock;

    private readonly Mock<ILogger<EfCoreRepository<TestEntity>>> _loggerMock;

    private readonly Mock<IEntityLifecycleProcessor> _lifecycleProcessorMock;

    private readonly Mock<ICursorSerializer> _cursorSerializerMock;

    private readonly List<SqliteConnection> _connections = [];

    /// <summary>
    /// A test entity for repository tests.
    /// </summary>
    public class TestEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// A test entity that implements <see cref="IAuditable"/> and <see cref="ISoftDelete"/>.
    /// </summary>
    public class AuditableSoftDeleteEntity : IAuditable, ISoftDelete
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt { get; set; }

        /// <inheritdoc />
        public string? CreatedBy { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <inheritdoc />
        public string? UpdatedBy { get; set; }

        /// <inheritdoc />
        public bool IsDeleted { get; set; }

        /// <inheritdoc />
        public DateTimeOffset? DeletedAt { get; set; }

        /// <inheritdoc />
        public string? DeletedBy { get; set; }
    }

    /// <summary>
    /// A test database context for repository tests.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TestsDbContext"/> class.
    /// </remarks>
    /// <param name="options">The database context options.</param>
    private class TestsDbContext(DbContextOptions<EfCoreRepositoryTests.TestsDbContext> options) : DbContext(options)
    {

        /// <summary>
        /// Gets or sets the test entities.
        /// </summary>
        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        /// <summary>
        /// Gets or sets the auditable soft-delete entities.
        /// </summary>
        public DbSet<AuditableSoftDeleteEntity> AuditableSoftDeleteEntities { get; set; } = null!;

        /// <inheritdoc />
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

    /// <summary>
    /// Initializes a new instance of the <see cref="EfCoreRepositoryTests"/> class.
    /// </summary>
    public EfCoreRepositoryTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _auditProviderMock = _fixture.Freeze<Mock<IAuditProvider>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<EfCoreRepository<TestEntity>>>>();
        _lifecycleProcessorMock = _fixture.Freeze<Mock<IEntityLifecycleProcessor>>();
        _cursorSerializerMock = _fixture.Freeze<Mock<ICursorSerializer>>();
    }

    /// <summary>
    /// Helper method to create a system under test with a SQLite in-memory database.
    /// </summary>
    private (EfCoreRepository<TestEntity> sut, TestsDbContext context) CreateSutWithSqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        _connections.Add(connection);

        var options = new DbContextOptionsBuilder<TestsDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new TestsDbContext(options);

        // Rationale: Ensure the schema is created before running any operations.
        context.Database.EnsureCreated();

        var sut = new EfCoreRepository<TestEntity>(
            context,
            _loggerMock.Object,
            _cursorSerializerMock.Object,
            _lifecycleProcessorMock.Object);

        return (sut, context);
    }

    /// <summary>
    /// Helper method to create an auditable system under test with a SQLite in-memory database.
    /// </summary>
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

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var connection in _connections)
        {
            connection.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreRepository{TEntity}.AddAsync"/> correctly adds a new entity to the database.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="EfCoreRepository{TEntity}.UpdateAsync"/> correctly updates an existing entity.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="EfCoreRepository{TEntity}.DeleteAsync"/> correctly removes an entity from the database.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="EfCoreRepository{TEntity}.ExecuteUpdateAsync"/> correctly updates multiple entities based on a condition.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="EfCoreRepository{TEntity}.ExecuteUpdateAsync"/> correctly updates audit fields for auditable entities.
    /// </summary>
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

        // Rationale: Setup processor to simulate auditing logic for bulk updates.
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

    /// <summary>
    /// Verifies that <see cref="EfCoreRepository{TEntity}.ExecuteDeleteAsync"/> correctly deletes multiple entities based on a condition.
    /// </summary>
    [Fact]
    public async Task ExecuteDeleteAsync_ConditionMet_DeletesEntities()
    {
        // Arrange
        var entity1 = _fixture.Build<TestEntity>().With(x => x.Id, 1).With(x => x.Value, 10).Create();
        var entity2 = _fixture.Build<TestEntity>().With(x => x.Id, 2).With(x => x.Value, 20).Create();
        var (sut, context) = CreateSutWithSqlite();
        context.TestEntities.AddRange(entity1, entity2);
        await context.SaveChangesAsync();

        var utcNow = DateTime.UtcNow;

        // Act
        var count = await sut.ExecuteDeleteAsync(e => e.Value == 10);

        // Assert
        count.Should().Be(1);
        context.ChangeTracker.Clear();
        context.TestEntities.Count().Should().Be(1);
        context.TestEntities.FirstOrDefault()!.Id.Should().Be(2);
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreRepository{TEntity}.ExecuteDeleteAsync"/> performs a soft-delete for entities that implement <see cref="ISoftDelete"/>.
    /// </summary>
    [Fact]
    public async Task ExecuteDeleteAsync_SoftDeleteEntity_PerformsSoftDelete()
    {
        // Arrange
        var entity = new AuditableSoftDeleteEntity { Id = 1, Name = "Original" };
        var (sut, context) = CreateAuditableSutWithSqlite();

        context.AuditableSoftDeleteEntities.Add(entity);
        await context.SaveChangesAsync();

        var utcNow = DateTime.UtcNow;

        // Rationale: Setup processor to simulate soft delete logic for bulk deletes.
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

        // Rationale: Should still exist in DB but marked as deleted. Use IgnoreQueryFilters to find it.
        var softDeleted = await context.AuditableSoftDeleteEntities.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == 1);
        softDeleted.Should().NotBeNull();
        softDeleted!.IsDeleted.Should().BeTrue();
        softDeleted.DeletedAt.Should().Be(utcNow);

        _lifecycleProcessorMock.Verify(p => p.ProcessBulkSoftDelete(It.IsAny<IPropertySetter<AuditableSoftDeleteEntity>>()), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreRepository{TEntity}.AddRangeAsync"/> correctly adds multiple entities to the database.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="EfCoreRepository{TEntity}.UpdateRangeAsync"/> correctly updates multiple existing entities.
    /// </summary>
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
        {
            e.Name += "_Updated";
        }

        await sut.UpdateRangeAsync(entities);
        await context.SaveChangesAsync();

        // Assert
        var dbEntities = await context.TestEntities.ToListAsync();
        dbEntities.Should().AllSatisfy(e => e.Name.Should().EndWith("_Updated"));
    }

    /// <summary>
    /// Verifies that <see cref="EfCoreRepository{TEntity}.DeleteRangeAsync"/> correctly removes multiple entities from the database.
    /// </summary>
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

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/> when the context is null.
    /// </summary>
    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        // Act
        Action act = () =>
        {
            _ = new EfCoreRepository<TestEntity>(null!, _loggerMock.Object, _cursorSerializerMock.Object, _lifecycleProcessorMock.Object);
        };

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
