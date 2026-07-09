using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Unit of Work interface for managing transactions and repositories.
/// </summary>
public interface IVKUnitOfWork : IDisposable, IAsyncDisposable
{

    /// <summary>
    /// Gets the current active transaction, if any.
    /// </summary>
    IVKTransaction? CurrentTransaction { get; }

    /// <summary>
    /// Determines whether the unit of work has any pending changes.
    /// </summary>
    /// <returns><c>true</c> if there are pending changes; otherwise, <c>false</c>.</returns>
    bool HasChanges();

    /// <summary>
    /// Asynchronously saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the number of state entries written to the database.</returns>
    // [CS.01]
    Task<VKResult<int>> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously begins a new transaction.
    /// </summary>
    // [CS.01]
    Task<VKResult<IVKTransaction>> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously commits the current transaction.
    /// </summary>
    // [CS.01]
    Task<VKResult> CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously rolls back the current transaction.
    /// </summary>
    // [CS.01]
    Task<VKResult> RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the repository for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>The repository instance for the specified entity type.</returns>
    IVKBaseRepository<TEntity> Repository<TEntity>() where TEntity : class;

}
