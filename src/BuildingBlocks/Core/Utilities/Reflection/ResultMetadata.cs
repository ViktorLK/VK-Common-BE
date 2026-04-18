using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Core.Utilities.Reflection;

/// <summary>
/// Provides high-performance, non-generic access to <see cref="Result{T}"/> failure creation (Rule 4).
/// Caches compiled expression delegates to avoid reflection overhead in hot paths.
/// </summary>
public static class ResultMetadata
{
    private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Error>, object>> _failureFactories = new();

    private static readonly MethodInfo _genericResultFailureMethodInfo = typeof(Result)
        .GetMethod(nameof(Result.Failure), genericParameterCount: 1, types: [typeof(IEnumerable<Error>)])
        ?? throw new InvalidOperationException("CRITICAL ERROR: Method Result.Failure<T>(IEnumerable<Error>) not found. API Contract broken.");

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> instance for the specified result type.
    /// </summary>
    /// <param name="resultType">The generic type argument for the Result.</param>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A boxed <see cref="Result{T}"/> containing the errors.</returns>
    public static object CreateFailure(Type resultType, IEnumerable<Error> errors)
    {
        Func<IEnumerable<Error>, object> factory = _failureFactories.GetOrAdd(resultType, t =>
        {
            MethodInfo genericMethod = _genericResultFailureMethodInfo.MakeGenericMethod(t);

            ParameterExpression param = Expression.Parameter(typeof(IEnumerable<Error>), "errors");
            MethodCallExpression call = Expression.Call(null, genericMethod, param);
            UnaryExpression cast = Expression.Convert(call, typeof(object));
            var lambda = Expression.Lambda<Func<IEnumerable<Error>, object>>(cast, param);

            return lambda.Compile();
        });

        return factory(errors);
    }
}

