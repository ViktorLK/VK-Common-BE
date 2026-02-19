using System.Linq.Expressions;
using VK.Blocks.Persistence.Core.Pagination;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Persistence.EFCore.Extensions;

/// <summary>
/// Extension methods for <see cref="IQueryable{T}"/>.
/// </summary>
public static class QueryableExtensions
{
    #region Public Methods

    /// <summary>
    /// Conditionally applies a Where clause to the query.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="condition">The condition to check.</param>
    /// <param name="predicate">The predicate to apply if the condition is true.</param>
    /// <returns>The query with the predicate applied if the condition is true; otherwise, the original query.</returns>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Conditionally applies an OrderBy or OrderByDescending clause to the query.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <typeparam name="TKey">The type of the key selector.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="condition">The condition to check.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="ascending">True to sort in ascending order; false for descending.</param>
    /// <returns>The sorted query if the condition is true; otherwise, the original query.</returns>
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
    /// Sorts the query based on cursor direction and original sort order.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <typeparam name="TKey">The type of the key selector.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="originalAscending">Whether the original sort was ascending.</param>
    /// <param name="cursorSelector">The cursor selector expression.</param>
    /// <param name="direction">The direction of the cursor pagination.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<T> OrderByCursorDirection<T, TKey>(
        this IQueryable<T> query,
        bool originalAscending,
        Expression<Func<T, TKey>> cursorSelector,
        CursorDirection direction)
        where TKey : IComparable<TKey>
    {
        // Rationale: When moving backwards, we need to invert the sort order to fetch the correct page,
        // then reverse the results in memory (handled by caller).
        var finalAscending = direction == CursorDirection.Forward
            ? originalAscending
            : !originalAscending;

        return finalAscending
            ? query.OrderBy(cursorSelector)
            : query.OrderByDescending(cursorSelector);
    }

    #endregion
}
