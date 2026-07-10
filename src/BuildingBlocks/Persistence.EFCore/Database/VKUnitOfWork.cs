using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Blocks.Persistence.EFCore.Database.Internal;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Unit of Work implementation for EF Core.
/// </summary>
public sealed class VKUnitOfWork<TDbContext>(TDbContext context, IServiceProvider serviceProvider) : IVKUnitOfWork<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext _context = VKGuard.NotNull(context);
    private readonly IServiceProvider _serviceProvider = VKGuard.NotNull(serviceProvider);
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    /// <inheritdoc />
    public IVKTransaction? CurrentTransaction =>
        _currentTransaction is null
            ? null
            : new EFCoreTransactionAdapter(_currentTransaction);

    /// <inheritdoc />
    public async Task<VKResult<int>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            var count = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false); // [CS.03]
            return VKResult.Success(count);
        }
        catch (DbUpdateConcurrencyException)
        {
            return VKResult.Failure<int>(VKPersistenceErrors.UnitOfWork.ConcurrentUpdate);
        }
        catch (Exception)
        {
            return VKResult.Failure<int>(VKPersistenceErrors.UnitOfWork.SaveChangesFailed);
        }
    }

    /// <inheritdoc />
    public async Task<VKResult<IVKTransaction>> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            return VKResult.Failure<IVKTransaction>(VKPersistenceErrors.Transaction.AlreadyActive);
        }

        try
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false); // [CS.03]
            return VKResult.Success<IVKTransaction>(new EFCoreTransactionAdapter(_currentTransaction));
        }
        catch (Exception)
        {
            return VKResult.Failure<IVKTransaction>(VKPersistenceErrors.Transaction.BeginFailed);
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> ExecuteInTransactionAsync(
        Func<IVKUnitOfWork<TDbContext>, CancellationToken, Task> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async (ct) =>
            {
                var transactionResult = await BeginTransactionAsync(isolationLevel, ct).ConfigureAwait(false); // [CS.03]
                if (transactionResult.IsFailure)
                {
                    return VKResult.Failure(transactionResult.FirstError);
                }

                try
                {
                    await operation(this, ct).ConfigureAwait(false); // [CS.03]
                    var commitResult = await CommitTransactionAsync(ct).ConfigureAwait(false); // [CS.03]
                    if (commitResult.IsFailure)
                    {
                        throw new InvalidOperationException("Transaction commit failed.");
                    }
                    return VKResult.Success();
                }
                catch (Exception)
                {
                    await RollbackTransactionAsync(ct).ConfigureAwait(false); // [CS.03]
                    throw;
                }
            }, cancellationToken).ConfigureAwait(false); // [CS.03]
        }
        catch (Exception)
        {
            return VKResult.Failure(VKPersistenceErrors.Transaction.CommitFailed);
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return VKResult.Failure(VKPersistenceErrors.Transaction.NoActiveTransaction);
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false); // [CS.03]
            return VKResult.Success();
        }
        catch (Exception)
        {
            return VKResult.Failure(VKPersistenceErrors.Transaction.CommitFailed);
        }
        finally
        {
            await DisposeTransactionAsync().ConfigureAwait(false); // [CS.03]
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return VKResult.Failure(VKPersistenceErrors.Transaction.NoActiveTransaction);
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false); // [CS.03]
            return VKResult.Success();
        }
        catch (Exception)
        {
            return VKResult.Failure(VKPersistenceErrors.Transaction.NoActiveTransaction);
        }
        finally
        {
            await DisposeTransactionAsync().ConfigureAwait(false); // [CS.03]
        }
    }

    /// <inheritdoc />
    public IVKBaseRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _serviceProvider.GetRequiredService<IVKBaseRepository<TEntity>>();
    }

    /// <inheritdoc />
    public bool HasChanges()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _context.ChangeTracker.HasChanges();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _currentTransaction?.Dispose();
        _disposed = true;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
        }

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
            _currentTransaction = null;
        }
    }
}
