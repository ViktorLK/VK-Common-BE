using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace VK.Blocks.Core;

/// <summary>
/// Provides high-performance caching for compiled expressions using Static Generic Caching.
/// </summary>
public sealed class VKExpressionCache
{
    // Non-generic outer class avoids CA1000 warnings while maintaining high-performance caching.
    private VKExpressionCache() { }

    /// <summary>
    /// Gets the compiled delegate for the expression, compiling it if necessary.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<TInput, TResult> GetOrCompile<TInput, TResult>(Expression<Func<TInput, TResult>> expression)
    {
        // Delegate the actual storage to a private nested generic class.
        return InnerCache<TInput, TResult>.Storage.GetOrAdd(expression, static expr => expr.Compile());
    }

    /// <summary>
    /// Gets the number of cached items for the specific type combination.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetCount<TInput, TResult>()
        => InnerCache<TInput, TResult>.Storage.Count;

    /// <summary>
    /// Clears the cache for the specific type combination.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear<TInput, TResult>()
        => InnerCache<TInput, TResult>.Storage.Clear();

    /// <summary>
    /// Nested generic class to hold the static field per type combination.
    /// Private class members do not trigger CA1000.
    /// </summary>
    private static class InnerCache<TIn, TOut>
    {
        // A separate ConcurrentDictionary instance is created per type combination by the runtime.
        internal static readonly ConcurrentDictionary<Expression<Func<TIn, TOut>>, Func<TIn, TOut>> Storage
            = new(VKExpressionEqualityComparer.Instance);
    }
}
