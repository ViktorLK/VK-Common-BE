using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace VK.Blocks.Persistence.EFCore.Storage.Internal;

/// <summary>
/// Adapter for EF Core transactions to <see cref="IVKTransaction"/>.
/// </summary>
internal sealed class EfCoreTransactionAdapter(IDbContextTransaction transaction) : IVKTransaction
{

    /// <inheritdoc />
    public Guid TransactionId => transaction.TransactionId;



    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
        => await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
        => await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public void Dispose() => transaction.Dispose();

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await transaction.DisposeAsync().ConfigureAwait(false);

}
