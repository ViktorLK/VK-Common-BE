using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using VK.Blocks.APIStandards.Shared;

namespace VK.Blocks.APIStandards.Caches;

internal static class ValidationFailureCache
{
    // Lazy Load
    private static readonly Lazy<MethodInfo> _genericResultFailureMethodInfo = new(() =>
    {
        return typeof(Result).GetMethod(nameof(Result.Failure),　genericParameterCount: 1,　types: [typeof(IEnumerable<Error>)])
                ?? throw new InvalidOperationException($"CRITICAL EORROR:Method {nameof(Result.Failure)}<T>(IEnumerable<Error>) not found on type {nameof(Result)} Method Result.Failure not found. API Contract broken.");
    });

    private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Error>, object>> _failureFactories = new();

    public static Func<IEnumerable<Error>, object> GetOrAdd(Type resultType)
    {
        return _failureFactories.GetOrAdd(resultType, t =>
        {
            var genericMethod = _genericResultFailureMethodInfo.Value.MakeGenericMethod(t);

            var param = Expression.Parameter(typeof(IEnumerable<Error>), "errors");
            var call = Expression.Call(null, genericMethod, param);
            var cast = Expression.Convert(call, typeof(object));
            var lambda = Expression.Lambda<Func<IEnumerable<Error>, object>>(cast, param);

            return lambda.Compile();
        });
    }
}
