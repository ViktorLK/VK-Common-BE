using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using VK.Blocks.Core.Caches.Internal;

namespace VK.Blocks.Core.Caches;

/// <summary>
/// Cache for compiled expressions to improve performance.
/// </summary>
/// <typeparam name="TInput">The input type of the expression.</typeparam>
/// <typeparam name="TResult">The result type of the expression.</typeparam>
public sealed class ExpressionCache<TInput, TResult>
{
    private static readonly ConcurrentDictionary<Expression<Func<TInput, TResult>>, Func<TInput, TResult>> _compiledExpressions
        = new(ExpressionEqualityComparer.Instance);

    private ExpressionCache()
    {
    }

    /// <summary>
    /// Gets the number of cached items.
    /// </summary>
    public static int CachedCount => _compiledExpressions.Count;

    /// <summary>
    /// Gets the compiled delegate for the expression, compiling it if necessary.
    /// </summary>
    /// <param name="expression">The expression to compile.</param>
    /// <returns>The compiled delegate.</returns>
    public static Func<TInput, TResult> GetOrCompile(Expression<Func<TInput, TResult>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return _compiledExpressions.GetOrAdd(expression, expr => expr.Compile());
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public static void Clear() => _compiledExpressions.Clear();
}
