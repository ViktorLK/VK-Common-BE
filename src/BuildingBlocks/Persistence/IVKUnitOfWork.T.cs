using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Persistence;

/// <summary>
/// Defines a typed Unit of Work for a specific database context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public interface IVKUnitOfWork<TDbContext> : IVKUnitOfWork
{
    /// <summary>
    /// Executes the specified operation within a transaction, handling transient failures via an Execution Strategy.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<IVKUnitOfWork<TDbContext>, CancellationToken, Task> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}
