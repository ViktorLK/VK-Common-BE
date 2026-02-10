using System.Linq.Expressions;

namespace VK.Blocks.Persistence.EFCore.Extensions;

/// <summary>
/// IQueryable 拡張メソッド
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// 条件フィルタリング - 条件が true の場合のみ Where を適用
    /// </summary>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
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
}


public class CursorPagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public string? NextCursor { get; init; }
    public bool HasNextPage { get; init; }
}
