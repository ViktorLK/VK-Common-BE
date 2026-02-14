using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace VK.Blocks.Persistence.EFCore.Caches;

internal static class EfCoreExpressionCache<TEntity, TResult>
{
    #region Fields

    public static readonly ConcurrentDictionary<Expression<Func<TEntity, TResult>>, Func<TEntity, TResult>> _compiledExpressions
        = new(ExpressionEqualityComparer.Instance);

    #endregion

    #region Properties

    public static int CachedCount => _compiledExpressions.Count;

    #endregion

    #region Public Methods

    public static Func<TEntity, TResult> GetOrCompile(Expression<Func<TEntity, TResult>> expression)
    {
        return _compiledExpressions.GetOrAdd(expression, expr => expr.Compile());
    }

    public static void Clear() => _compiledExpressions.Clear();

    #endregion
}

