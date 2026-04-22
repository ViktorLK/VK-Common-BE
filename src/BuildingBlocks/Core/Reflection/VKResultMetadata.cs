using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace VK.Blocks.Core;

/// <summary>
/// Provides high-performance, non-generic access to <see cref="VKResult{T}"/> failure creation (Rule 4).
/// Caches compiled expression delegates to avoid reflection overhead in hot paths.
/// </summary>
public static class VKResultMetadata
{
    private static readonly ConcurrentDictionary<Type, Func<IEnumerable<VKError>, object>> _failureFactories = new();

    private static readonly MethodInfo _genericResultFailureMethodInfo = typeof(VKResult)
        .GetMethod(nameof(VKResult.Failure), genericParameterCount: 1, types: [typeof(IEnumerable<VKError>)])
        ?? throw new InvalidOperationException("CRITICAL ERROR: Method VKResult.Failure<T>(IEnumerable<VKError>) not found. API Contract broken.");

    /// <summary>
    /// Creates a failed <see cref="VKResult{T}"/> instance for the specified result type.
    /// </summary>
    /// <param name="resultType">The generic type argument for the VKResult.</param>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A boxed <see cref="VKResult{T}"/> containing the errors.</returns>
    public static object CreateFailure(Type resultType, IEnumerable<VKError> errors)
    {
        Func<IEnumerable<VKError>, object> factory = _failureFactories.GetOrAdd(resultType, t =>
        {
            MethodInfo genericMethod = _genericResultFailureMethodInfo.MakeGenericMethod(t);

            ParameterExpression param = Expression.Parameter(typeof(IEnumerable<VKError>), "errors");
            MethodCallExpression call = Expression.Call(null, genericMethod, param);
            UnaryExpression cast = Expression.Convert(call, typeof(object));
            var lambda = Expression.Lambda<Func<IEnumerable<VKError>, object>>(cast, param);

            return lambda.Compile();
        });

        return factory(errors);
    }
}
