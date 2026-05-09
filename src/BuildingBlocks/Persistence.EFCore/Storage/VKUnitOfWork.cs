using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Storage.Internal;

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
            : new EfCoreTransactionAdapter(_currentTransaction);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IVKTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException("A transaction is already active.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
        return new EfCoreTransactionAdapter(_currentTransaction);
    }

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(
        Func<IVKUnitOfWork<TDbContext>, CancellationToken, Task> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        // Use the configured execution strategy (e.g. SQL Server retry policy)
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async (ct) =>
        {
            await using var transaction = await BeginTransactionAsync(isolationLevel, ct).ConfigureAwait(false);

            try
            {
                await operation(this, ct).ConfigureAwait(false);
                await CommitTransactionAsync(ct).ConfigureAwait(false);
            }
            catch
            {
                await RollbackTransactionAsync(ct).ConfigureAwait(false);
                throw;
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await DisposeTransactionAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await DisposeTransactionAsync().ConfigureAwait(false);
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
