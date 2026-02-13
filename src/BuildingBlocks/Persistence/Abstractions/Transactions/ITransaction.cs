namespace VK.Blocks.Persistence.Abstractions.Transactions;

/// <summary>
/// Transaction interface for managing database transactions.
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
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
    /// Rollbacks the transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
