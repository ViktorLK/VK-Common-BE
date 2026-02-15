using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Persistence.Abstractions;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.Abstractions.Transactions;
using VK.Blocks.Persistence.EFCore.Adapters;
using VK.Blocks.Persistence.EFCore.Constants;
using VK.Blocks.Persistence.EFCore.Repositories;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Unit of Work implementation for EF Core.
/// </summary>
public class UnitOfWork<TDbContext>(
    TDbContext context,
    IServiceProvider serviceProvider)
    : IUnitOfWork<TDbContext>
    where TDbContext : DbContext
{
    #region Fields

    private readonly TDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    #endregion

    #region Properties

    /// <inheritdoc />
    public ITransaction? CurrentTransaction =>
        _currentTransaction is null
            ? null
            : new EfCoreTransactionAdapter(_currentTransaction);

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement isolation level support
        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException(RepositoryConstants.ErrorMessages.TransactionAlreadyActive);
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        return new EfCoreTransactionAdapter(_currentTransaction);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            throw new InvalidOperationException(RepositoryConstants.ErrorMessages.NoActiveTransaction);
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            throw;
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
    public IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _serviceProvider.GetRequiredService<IBaseRepository<TEntity>>();
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _currentTransaction?.Dispose();
        }

        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion

    #region Private Methods

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
            _currentTransaction = null;
        }
    }

    #endregion
}
