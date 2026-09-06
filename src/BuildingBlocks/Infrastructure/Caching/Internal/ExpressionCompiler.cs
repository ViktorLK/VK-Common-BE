using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace VK.Blocks.Infrastructure.Caching.Internal;

/// <summary>
/// An engine for compiling and caching Linq Expressions into high-performance delegates.
/// </summary>
internal static class ExpressionCompiler
{
    private static readonly ConcurrentDictionary<(Type Type, string Member, string Operation), Delegate> _delegateCache = new();

    /// <summary>
    /// Gets or compiles a delegate from a unique key.
    /// </summary>
    public static TDelegate GetOrCompile<TDelegate>(Type type, string memberName, string operation, Func<Expression<TDelegate>> factory)
        where TDelegate : Delegate
    {
        var key = (type, memberName, operation);
        return (TDelegate)_delegateCache.GetOrAdd(key, _ =>
        {
            var expression = factory();
            return expression.Compile();
        });
    }
}
