
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.Abstractions.Transactions;

namespace VK.Blocks.Persistence.Abstractions;

/// <summary>
/// Unit of Work interface for managing transactions and repositories.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the current transaction.
    /// </summary>
    ITransaction? CurrentTransaction { get; }

    /// <summary>
    /// Checks if there are any pending changes.
    /// </summary>
    bool HasChanges();

    /// <summary>
    /// Saves all changes to the underlying data store.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a repository for the specified entity type.
    /// </summary>
    IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class;

    // Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
    // Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);
}
public interface IUnitOfWork<TDbContext> : IUnitOfWork
{
}
