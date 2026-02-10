using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VK.Blocks.Persistence.EFCore.Constants;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Unit of Work インターフェース
/// 特徴：トランザクション管理、SOLID 原則における単一責任
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// すべての変更を保存
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// トランザクションを開始
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// トランザクションをコミット
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// トランザクションをロールバック
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定されたエンティティのリポジトリを取得
    /// </summary>
    IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class;
}

/// <summary>
/// Unit of Work 実装
/// 特徴：
/// 1. プライマリコンストラクタ (C# 12)
/// 2. IDisposable と IAsyncDisposable パターン
/// 3. トランザクション管理
/// 4. リポジトリファクトリパターン
/// </summary>
public class UnitOfWork(DbContext context) : IUnitOfWork
{
    private readonly DbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private IDbContextTransaction? _currentTransaction;
    private readonly Dictionary<Type, object> _repositories = new();
    private bool _disposed;

    // ========== 保存操作 ==========

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    // ========== トランザクション管理 ==========

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException(RepositoryConstants.ErrorMessages.TransactionAlreadyActive);
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            throw new InvalidOperationException(RepositoryConstants.ErrorMessages.NoActiveTransaction);
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
            _currentTransaction = null;
        }
    }

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
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
            _currentTransaction = null;
        }
    }

    // ========== リポジトリファクトリ ==========

    /// <summary>
    /// リポジトリインスタンスを取得（シングルトンパターン、各エンティティ型につき一度だけ作成）
    /// </summary>
    public IBaseRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if (_repositories.TryGetValue(type, out var existingRepo))
        {
            return (IBaseRepository<TEntity>)existingRepo;
        }

        var repository = new BaseRepository<TEntity>(_context);
        _repositories[type] = repository;

        return repository;
    }

    // ========== Dispose パターン ==========

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _currentTransaction?.Dispose();
            _repositories.Clear();
        }

        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
        }

        _repositories.Clear();
    }
}
