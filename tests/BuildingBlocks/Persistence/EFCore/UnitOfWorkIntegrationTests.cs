using System;
using System.Threading.Tasks;
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

/// <summary>
/// Integration tests for <see cref="UnitOfWork{TContext}"/>.
/// </summary>
public class UnitOfWorkIntegrationTests : IntegrationTestBase<TestDbContext>
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWorkIntegrationTests"/> class.
    /// </summary>
    public UnitOfWorkIntegrationTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
    }

    /// <summary>
    /// Verifies that <see cref="UnitOfWork{TContext}.HasChanges"/> returns true when there are pending changes in the context.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="UnitOfWork{TContext}.BeginTransactionAsync(System.Threading.CancellationToken)"/> starts a database transaction.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="UnitOfWork{TContext}.CommitTransactionAsync(System.Threading.CancellationToken)"/> commits the changes to the database.
    /// </summary>
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
        // Rationale: Clear the tracker to ensure we are reading from the database and not the local cache.
        Context.ChangeTracker.Clear();
        var saved = await Context.Products.FirstOrDefaultAsync(p => p.Name == "Committed");
        saved.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UnitOfWork{TContext}.RollbackTransactionAsync(System.Threading.CancellationToken)"/> rolls back the changes.
    /// </summary>
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

        // Rationale: Verify it exists in context before rollback (and in DB within transaction)
        Context.Products.Local.Should().Contain(product);

        await sut.RollbackTransactionAsync();

        // Assert
        // Rationale: Clear the tracker to verify persistence is rolled back in the DB.
        Context.ChangeTracker.Clear();
        var saved = await Context.Products.FirstOrDefaultAsync(p => p.Name == "RolledBack");
        saved.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UnitOfWork{TContext}.Repository{TEntity}"/> resolves the repository from the service provider.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="UnitOfWork{TContext}.CommitTransactionAsync(System.Threading.CancellationToken)"/> throws when no transaction is active.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="UnitOfWork{TContext}.ExecuteInTransactionAsync(Func{IUnitOfWork, System.Threading.CancellationToken, Task}, System.Threading.CancellationToken)"/> commits on success.
    /// </summary>
    [Fact]
    public async Task ExecuteInTransactionAsync_Success_Commits()
    {
        // Arrange
        var sut = new UnitOfWork<TestDbContext>(Context, _serviceProviderMock.Object);

        // Act
        await sut.ExecuteInTransactionAsync(async (uow, ct) =>
        {
            Context.Products.Add(new TestProduct { Name = "InTransaction" });
            await uow.SaveChangesAsync(ct);
        });

        // Assert
        Context.ChangeTracker.Clear();
        var saved = await Context.Products.FirstOrDefaultAsync(p => p.Name == "InTransaction");
        saved.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UnitOfWork{TContext}.ExecuteInTransactionAsync(Func{IUnitOfWork, System.Threading.CancellationToken, Task}, System.Threading.CancellationToken)"/> rolls back on failure.
    /// </summary>
    [Fact]
    public async Task ExecuteInTransactionAsync_Failure_RollsBack()
    {
        // Arrange
        var sut = new UnitOfWork<TestDbContext>(Context, _serviceProviderMock.Object);

        // Act
        Func<Task> act = async () => await sut.ExecuteInTransactionAsync(async (uow, ct) =>
        {
            Context.Products.Add(new TestProduct { Name = "ShouldRollback" });
            await uow.SaveChangesAsync(ct);
            throw new Exception("Failure");
        });

        // Assert
        await act.Should().ThrowAsync<Exception>();
        Context.ChangeTracker.Clear();
        var saved = await Context.Products.FirstOrDefaultAsync(p => p.Name == "ShouldRollback");
        saved.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UnitOfWork{TContext}.DisposeAsync"/> disposes the transaction.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_DisposesTransaction()
    {
        // Arrange
        var sut = new UnitOfWork<TestDbContext>(Context, _serviceProviderMock.Object);
        await sut.BeginTransactionAsync();

        // Act
        await sut.DisposeAsync();

        // Assert
        Context.Database.CurrentTransaction.Should().BeNull();
    }
}
