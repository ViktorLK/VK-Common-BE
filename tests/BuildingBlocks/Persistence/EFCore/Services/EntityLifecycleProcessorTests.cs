using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VK.Blocks.Persistence.Abstractions.Auditing;
using VK.Blocks.Core.Results;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.EFCore.Repositories;
using VK.Blocks.Persistence.EFCore.Services;
using VK.Blocks.Persistence.EFCore.Tests; // For TestDbContext and TestProduct
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Services;

public class EntityLifecycleProcessorTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly Mock<IAuditProvider> _auditProviderMock;
    private readonly EntityLifecycleProcessor _sut;
    private readonly TestDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;

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
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void ProcessAuditing_NullContext_ThrowsArgumentNullException()
    {
        Action act = () => _sut.ProcessAuditing(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProcessSoftDelete_NullContext_ThrowsArgumentNullException()
    {
        Action act = () => _sut.ProcessSoftDelete(null!);
        act.Should().Throw<ArgumentNullException>();
    }

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
        // Note: In InMemory, Add might not set state until DetectChanges or SaveChanges context depending on tracking
        // But Products.Add() usually sets it to Added.

        // Act
        _sut.ProcessAuditing(_context);

        // Assert
        entity.CreatedAt.Should().Be(utcNow);
        entity.CreatedBy.Should().Be(userId);
    }

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

        // IsModified check
        _context.Entry(entity).Property(x => x.CreatedAt).IsModified.Should().BeFalse();
        _context.Entry(entity).Property(x => x.CreatedBy).IsModified.Should().BeFalse();
    }

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
        entry.State.Should().Be(EntityState.Modified);
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().Be(utcNow);

        // IAuditable checks
        entity.UpdatedAt.Should().Be(utcNow);
        entity.UpdatedBy.Should().Be(userId);
    }

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
