using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Persistence;

/// <summary>
/// Transaction interface for managing database transactions.
/// </summary>
public interface IVKTransaction : IDisposable, IAsyncDisposable
{

    /// <summary>
    /// Gets the unique identifier of the transaction (for debugging).
    /// </summary>
    Guid TransactionId { get; }



    /// <summary>
    /// Commits the transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously rolls back the transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);

}
