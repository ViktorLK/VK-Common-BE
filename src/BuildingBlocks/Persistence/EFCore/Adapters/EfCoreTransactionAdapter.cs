using Microsoft.EntityFrameworkCore.Storage;
using VK.Blocks.Persistence.Abstractions.Transactions;

namespace VK.Blocks.Persistence.EFCore.Adapters;

/// <summary>
/// Adapter for EF Core transactions to <see cref="ITransaction"/>.
/// </summary>
internal class EfCoreTransactionAdapter(IDbContextTransaction transaction) : ITransaction
{
    #region Properties

    /// <inheritdoc />
    public Guid TransactionId => transaction.TransactionId;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken ct = default) => await transaction.CommitAsync(ct);

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken ct = default) => await transaction.RollbackAsync(ct);

    /// <inheritdoc />
    public void Dispose() => transaction.Dispose();

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await transaction.DisposeAsync();

    #endregion
}
