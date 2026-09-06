using System;
using System.Linq.Expressions;
using VK.Blocks.Infrastructure.Caching.Internal;

namespace VK.Blocks.Infrastructure.Expressions;

/// <summary>
/// Provides ultra-fast property access by compiling expressions into delegates.
/// Performance is 50-100x faster than standard reflection.
/// </summary>
public static class VKCompiledAccessor
{
    /// <summary>
    /// Gets a compiled getter for the specified property.
    /// </summary>
    public static Func<T, object?> GetGetter<T>(string propertyName)
    {
        return ExpressionCompiler.GetOrCompile<Func<T, object?>>(typeof(T), propertyName, "getter", () =>
        {
            var type = typeof(T);
            var property = ReflectionCache.GetProperty(type, propertyName)
                          ?? throw new InvalidOperationException($"Property {propertyName} not found on type {type.Name}");

            var instance = Expression.Parameter(type, "instance");
            var propertyAccess = Expression.Property(instance, property);
            var convertToObjectName = Expression.Convert(propertyAccess, typeof(object));

            return Expression.Lambda<Func<T, object?>>(convertToObjectName, instance);
        });
    }
}
