using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.EFCore.Services;
using VK.Blocks.Persistence.EFCore.Tests;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Services;

/// <summary>
/// Unit tests for <see cref="EntityLifecycleProcessor"/>.
/// </summary>
public class EntityLifecycleProcessorTests : IDisposable
{
    private readonly IFixture _fixture;

    private readonly Mock<IAuditProvider> _auditProviderMock;

    private readonly EntityLifecycleProcessor _sut;

    private readonly TestDbContext _context;

    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityLifecycleProcessorTests"/> class.
    /// </summary>
    public EntityLifecycleProcessorTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _auditProviderMock = _fixture.Freeze<Mock<IAuditProvider>>();
        _sut = new EntityLifecycleProcessor(_auditProviderMock.Object);

        _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new TestDbContext(options);

        // Rationale: Ensure the schema is created before running tests.
        _context.Database.EnsureCreated();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that <see cref="EntityLifecycleProcessor.ProcessAuditing"/> throws <see cref="ArgumentNullException"/> when the context is null.
    /// </summary>
    [Fact]
    public void ProcessAuditing_NullContext_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _sut.ProcessAuditing(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="EntityLifecycleProcessor.ProcessSoftDelete"/> throws <see cref="ArgumentNullException"/> when the context is null.
    /// </summary>
    [Fact]
    public void ProcessSoftDelete_NullContext_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _sut.ProcessSoftDelete(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="EntityLifecycleProcessor.ProcessAuditing"/> correctly sets audit fields for newly added entities.
    /// </summary>
    [Fact]
    public void ProcessAuditing_AddedEntity_SetsCreatedAndUpdatedFields()
    {
        // Arrange
        var utcNow = DateTimeOffset.UtcNow;
        var userId = "user1";
        _auditProviderMock.Setup(x => x.UtcNow).Returns(utcNow);
        _auditProviderMock.Setup(x => x.CurrentUserId).Returns(userId);

        var entity = new TestProduct { Name = "New", Price = 10, Id = Guid.NewGuid() };
        _context.Products.Add(entity);

        // Act
        _sut.ProcessAuditing(_context);

        // Assert
        entity.CreatedAt.Should().Be(utcNow);
        entity.CreatedBy.Should().Be(userId);
    }

    /// <summary>
    /// Verifies that <see cref="EntityLifecycleProcessor.ProcessAuditing"/> correctly sets updated fields for modified entities.
    /// </summary>
    [Fact]
    public void ProcessAuditing_ModifiedEntity_SetsUpdatedFields()
    {
        // Arrange
        var utcNow = DateTimeOffset.UtcNow;
        var userId = "user2";
        _auditProviderMock.Setup(x => x.UtcNow).Returns(utcNow);
        _auditProviderMock.Setup(x => x.CurrentUserId).Returns(userId);

        var entity = new TestProduct { Name = "Existing", Price = 10, Id = Guid.NewGuid() };
        _context.Products.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;

        // Act
        _sut.ProcessAuditing(_context);

        // Assert
        entity.UpdatedAt.Should().Be(utcNow);
        entity.UpdatedBy.Should().Be(userId);

        // Rationale: Ensure only updated fields are marked as modified.
        _context.Entry(entity).Property(x => x.CreatedAt).IsModified.Should().BeFalse();
        _context.Entry(entity).Property(x => x.CreatedBy).IsModified.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="EntityLifecycleProcessor.ProcessSoftDelete"/> correctly marks deleted entities as soft-deleted.
    /// </summary>
    [Fact]
    public void ProcessSoftDelete_DeletedEntity_SetsIsDeletedAndModified()
    {
        // Arrange
        var utcNow = DateTimeOffset.UtcNow;
        var userId = "user3";
        _auditProviderMock.Setup(x => x.UtcNow).Returns(utcNow);
        _auditProviderMock.Setup(x => x.CurrentUserId).Returns(userId);

        var entity = new TestProduct { Name = "ToDelete", Price = 10, Id = Guid.NewGuid() };
        _context.Products.Attach(entity);
        _context.Entry(entity).State = EntityState.Deleted;

        // Act
        _sut.ProcessSoftDelete(_context);

        // Assert
        var entry = _context.Entry(entity);

        // Rationale: After processing soft delete, the entity state should be changed from Deleted to Modified.
        entry.State.Should().Be(EntityState.Modified);
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().Be(utcNow);

        // Rationale: Verification for IAuditable integration if implemented.
        entity.UpdatedAt.Should().Be(utcNow);
        entity.UpdatedBy.Should().Be(userId);
    }

    /// <summary>
    /// Verifies that <see cref="EntityLifecycleProcessor.ProcessBulkUpdate"/> correctly triggers property updates in bulk.
    /// </summary>
    [Fact]
    public void ProcessBulkUpdate_AuditableEntity_CallsSetProperty()
    {
        // Arrange
        var utcNow = DateTimeOffset.UtcNow;
        var userId = "user4";
        _auditProviderMock.Setup(x => x.UtcNow).Returns(utcNow);
        _auditProviderMock.Setup(x => x.CurrentUserId).Returns(userId);

        var setterMock = new Mock<IPropertySetter<TestProduct>>();

        // Act
        _sut.ProcessBulkUpdate(setterMock.Object);

        // Assert
        setterMock.Verify(x => x.SetProperty(It.IsAny<System.Linq.Expressions.Expression<Func<TestProduct, DateTimeOffset?>>>(), utcNow), Times.Once);
        setterMock.Verify(x => x.SetProperty(It.IsAny<System.Linq.Expressions.Expression<Func<TestProduct, string?>>>(), userId), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="EntityLifecycleProcessor.ProcessBulkSoftDelete"/> correctly triggers property updates for bulk soft deletion.
    /// </summary>
    [Fact]
    public void ProcessBulkSoftDelete_SoftDeleteEntity_CallsSetProperty()
    {
        // Arrange
        var utcNow = DateTimeOffset.UtcNow;
        var userId = "user5";
        _auditProviderMock.Setup(x => x.UtcNow).Returns(utcNow);
        _auditProviderMock.Setup(x => x.CurrentUserId).Returns(userId);

        var setterMock = new Mock<IPropertySetter<TestProduct>>();

        // Act
        _sut.ProcessBulkSoftDelete(setterMock.Object);

        // Assert
        setterMock.Verify(x => x.SetProperty(It.IsAny<System.Linq.Expressions.Expression<Func<TestProduct, bool>>>(), true), Times.Once);
        setterMock.Verify(x => x.SetProperty(It.IsAny<System.Linq.Expressions.Expression<Func<TestProduct, DateTimeOffset?>>>(), utcNow), Times.AtLeastOnce);
        setterMock.Verify(x => x.SetProperty(It.IsAny<System.Linq.Expressions.Expression<Func<TestProduct, string?>>>(), userId), Times.Once);
    }
}
