using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using VK.Blocks.Persistence.EFCore.Extensions;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// 汎用リポジトリインターフェース
/// </summary>
/// <typeparam name="TEntity">エンティティ型、参照型である必要があります</typeparam>
public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetFirstOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetSingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetSingleOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetListAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        int pageNumber,
        int pageSize,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// cursorProperty必須有索引且唯一
    /// </summary>
    Task<CursorPagedResult<TEntity>> GetCursorPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> cursorProperty,
        TKey? lastValue,
        int pageSize,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void UpdateRange(IReadOnlyList<TEntity> entities);

    void Delete(TEntity entity);

    Task<int> ExecuteDeleteRangeAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> FromSqlRawAsync(
        string sql,
        CancellationToken cancellationToken = default,
        params object[] parameters);

    /// <summary>
    /// ストリーミング検索 - 大量データシナリオに適用
    /// </summary>
    IAsyncEnumerable<TEntity> StreamAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TResult>> ExecuteAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default);

    Task<TResult?> ExecuteSingleAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> builder,
        CancellationToken cancellationToken = default);
}
