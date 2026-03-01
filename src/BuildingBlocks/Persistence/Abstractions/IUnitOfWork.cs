using System.Data;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.Abstractions.Transactions;

namespace VK.Blocks.Persistence.Abstractions;

/// <summary>
/// Unit of Work interface for managing transactions and repositories.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    #region Properties

    /// <summary>
    /// Gets the current active transaction, if any.
    /// </summary>
    ITransaction? CurrentTransaction { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Determines whether the unit of work has any pending changes.
    /// </summary>
    /// <returns><c>true</c> if there are pending changes; otherwise, <c>false</c>.</returns>
    bool HasChanges();

    /// <summary>
    /// Asynchronously saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously begins a new transaction.
    /// </summary>
    Task<ITransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the repository for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>The repository instance for the specified entity type.</returns>
    IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class;

    #endregion
}

/// <summary>
/// Defines a typed Unit of Work for a specific database context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public interface IUnitOfWork<TDbContext> : IUnitOfWork
{
    /// <summary>
    /// Executes the specified operation within a transaction, handling transient failures via an Execution Strategy.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<IUnitOfWork<TDbContext>, CancellationToken, Task> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}
