using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VK.Blocks.Persistence.EFCore.Constants;

namespace VK.Blocks.Persistence.EFCore.Extensions;

/// <summary>
/// IQueryable 拡張メソッド
/// 特徴：
/// 1. 拡張メソッド
/// 2. 条件付き LINQ 構築
/// 3. 動的ソート
/// 4. ページング カプセル化
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// 条件フィルタリング - 条件が true の場合のみ Where を適用
    /// 用途：大量の if-else 判定を回避し、よりスムーズなクエリ構築を実現
    /// </summary>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    /// ページング拡張
    /// </summary>
    public static IQueryable<T> Paginate<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), RepositoryConstants.ErrorMessages.PageNumberMustBePositive);

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), RepositoryConstants.ErrorMessages.PageSizeMustBePositive);

        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// ページング結果のカプセル化（総数を含む）
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .Paginate(pageNumber, pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public static IOrderedQueryable<T> OrderByKeySelector<T, TKey>(
        this IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector,
        bool ascending)
    {
        return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
    }

    public static IQueryable<T> OrderByIf<T, TKey>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, TKey>> keySelector,
        bool ascending = true)
    {
        return !condition
            ? query
            : ascending
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);
    }

    /// <summary>
    /// 関連エンティティを含む（複数の Include をサポート）
    /// </summary>
    public static IQueryable<T> IncludeMultiple<T>(
        this IQueryable<T> query,
        params Expression<Func<T, object>>[] includes)
        where T : class
    {
        return includes.Aggregate(query, (current, include) => current.Include(include));
    }

    /// <summary>
    /// 条件付き Include
    /// </summary>
    public static IQueryable<T> IncludeIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, object>> navigationProperty)
        where T : class
    {
        return condition ? query.Include(navigationProperty) : query;
    }

    /// <summary>
    /// 重複排除（指定されたプロパティに基づく）
    /// </summary>
    public static IQueryable<T> DistinctBy<T, TKey>(
        this IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector)
    {
        return query.GroupBy(keySelector).Select(g => g.First());
    }

    /// <summary>
    /// 論理削除フィルタ（エンティティに IsDeleted プロパティがあることを想定）
    /// </summary>
    public static IQueryable<T> WhereNotDeleted<T>(this IQueryable<T> query)
        where T : class
    {
        var parameter = Expression.Parameter(typeof(T), RepositoryConstants.ExpressionParameterNames.Entity);
        var property = typeof(T).GetProperty(RepositoryConstants.PropertyNames.IsDeleted);

        if (property is null || property.PropertyType != typeof(bool))
            return query;

        var isDeletedProperty = Expression.Property(parameter, property);
        var condition = Expression.Equal(isDeletedProperty, Expression.Constant(false));
        var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);

        return query.Where(lambda);
    }
}

/// <summary>
/// ページング結果モデル
/// 特徴：Record、Init-only プロパティ
/// </summary>
public record PagedResult<T>
{
    public required IEnumerable<T> Items { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required int TotalPages { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}


public class CursorPagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public string? NextCursor { get; init; }
    public bool HasNextPage { get; init; }
}
