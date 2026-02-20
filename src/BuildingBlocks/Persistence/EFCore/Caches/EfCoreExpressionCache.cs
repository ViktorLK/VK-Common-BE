using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace VK.Blocks.Persistence.EFCore.Caches;

/// <summary>
/// Cache for compiled expressions to improve performance.
/// </summary>
internal static class EfCoreExpressionCache<TEntity, TResult>
{
    #region Fields

    /// <summary>
    /// The cache dictionary for compiled expressions.
    /// </summary>
    private static readonly ConcurrentDictionary<Expression<Func<TEntity, TResult>>, Func<TEntity, TResult>> _compiledExpressions
        = new(ExpressionEqualityComparer.Instance);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the number of cached items.
    /// </summary>
    public static int CachedCount => _compiledExpressions.Count;

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the compiled delegate for the expression, compiling it if necessary.
    /// </summary>
    /// <param name="expression">The expression to compile.</param>
    /// <returns>The compiled delegate.</returns>
    public static Func<TEntity, TResult> GetOrCompile(Expression<Func<TEntity, TResult>> expression)
    {
        return _compiledExpressions.GetOrAdd(expression, expr => expr.Compile());
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public static void Clear() => _compiledExpressions.Clear();

    #endregion
}

