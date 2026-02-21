using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.EFCore.Constants;
using VK.Blocks.Persistence.EFCore.IntegrationTests.Infrastructure;
using VK.Blocks.Persistence.EFCore.Tests;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests;

public class UnitOfWorkIntegrationTests : IntegrationTestBase<TestDbContext>
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public UnitOfWorkIntegrationTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
    }

    [Fact]
    public void HasChanges_ReturnsTrue_When_Changes_Exist()
    {
        // Arrange
        var sut = new UnitOfWork<TestDbContext>(Context, _serviceProviderMock.Object);

        // Act & Assert - Initially false
        sut.HasChanges().Should().BeFalse();

        // Act - Add entity
        Context.Products.Add(new TestProduct { Name = "Test" });

        // Assert - True
        sut.HasChanges().Should().BeTrue();
    }

    [Fact]
    public async Task BeginTransactionAsync_StartsTransaction()
    {
        // Arrange
        var sut = new UnitOfWork<TestDbContext>(Context, _serviceProviderMock.Object);

        // Act
        await sut.BeginTransactionAsync();

        // Assert
        sut.CurrentTransaction.Should().NotBeNull();
        Context.Database.CurrentTransaction.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitTransactionAsync_CommitsChanges()
    {
        // Arrange
        var sut = new UnitOfWork<TestDbContext>(Context, _serviceProviderMock.Object);
        await sut.BeginTransactionAsync();

        // Act
        var product = new TestProduct { Name = "Committed" };
        Context.Products.Add(product);
        await sut.SaveChangesAsync();
        await sut.CommitTransactionAsync();

        // Assert
        // Clear tracker to verify persistence
        Context.ChangeTracker.Clear();
        var saved = await Context.Products.FirstOrDefaultAsync(p => p.Name == "Committed");
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task RollbackTransactionAsync_RollsBackChanges()
    {
        // Arrange
        var sut = new UnitOfWork<TestDbContext>(Context, _serviceProviderMock.Object);
        await sut.BeginTransactionAsync();

        // Act
        var product = new TestProduct { Name = "RolledBack" };
        Context.Products.Add(product);
        await sut.SaveChangesAsync();

        // Verify it exists in context before rollback (and in DB within transaction)
        // Check local first
        Context.Products.Local.Should().Contain(product);

        await sut.RollbackTransactionAsync();

        // Assert
        Context.ChangeTracker.Clear(); // Clear local state
        var saved = await Context.Products.FirstOrDefaultAsync(p => p.Name == "RolledBack");
        saved.Should().BeNull();
    }

    [Fact]
    public void Repository_ResolvesFromServiceProvider()
    {
        // Arrange
        var sut = new UnitOfWork<TestDbContext>(Context, _serviceProviderMock.Object);
        var repoMock = new Mock<IBaseRepository<TestProduct>>();
        _serviceProviderMock.Setup(x => x.GetService(typeof(IBaseRepository<TestProduct>)))
            .Returns(repoMock.Object);

        // Act
        var repo = sut.Repository<TestProduct>();

        // Assert
        repo.Should().BeSameAs(repoMock.Object);
    }

    [Fact]
    public async Task CommitTransactionAsync_NoActiveTransaction_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new UnitOfWork<TestDbContext>(Context, _serviceProviderMock.Object);

        // Act
        Func<Task> act = async () => await sut.CommitTransactionAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(RepositoryConstants.ErrorMessages.NoActiveTransaction);
    }

    [Fact]
    public async Task BeginTransactionAsync_ActiveTransaction_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new UnitOfWork<TestDbContext>(Context, _serviceProviderMock.Object);
        await sut.BeginTransactionAsync();

        // Act
        Func<Task> act = async () => await sut.BeginTransactionAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(RepositoryConstants.ErrorMessages.TransactionAlreadyActive);
    }
}
