namespace VK.Blocks.Persistence.Abstractions.Transactions;

/// <summary>
/// Transaction interface for managing database transactions.
/// </summary>
public interface ITransaction : IDisposable, IAsyncDisposable
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier of the transaction (for debugging).
    /// </summary>
    Guid TransactionId { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously rolls back the transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    #endregion
}
