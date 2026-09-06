using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using VK.Blocks.Infrastructure.Caching.Internal;

namespace VK.Blocks.Infrastructure.Configuration.Internal;

/// <summary>
/// Enhanced configuration binder that supports zero-reflection concepts and hierarchical merging.
/// </summary>
internal static class VKConfigurationBinder
{
    /// <summary>
    /// Binds a section to an options object using the optimized binder.
    /// </summary>
    public static T BindOptions<T>(IConfigurationSection section) where T : class, new()
        => BindAndMerge<T>(section);

    /// <summary>
    /// Binds a section to an options object and optionally merges it with a base object.
    /// Supports the "Global Default + Local Override" pattern (AP.05).
    /// </summary>
    public static T BindAndMerge<T>(IConfigurationSection section, T? baseOptions = null) where T : class, new()
    {
        var options = section.Get<T>() ?? new T();

        if (baseOptions is null)
        {
            return options;
        }

        // AP.05: Hierarchical Merge Priority (Local ?? Global)
        // Since we are using records with init properties, we typically use 'with' expressions.
        // However, generic merging requires either reflection (cached) or generated code.
        return Merge(options, baseOptions);
    }

    private static T Merge<T>(T local, T @global) where T : class
    {
        var merger = ExpressionCompiler.GetOrCompile<Action<T, T>>(typeof(T), "merger", "merge", () =>
        {
            var target = Expression.Parameter(typeof(T), "target");
            var source = Expression.Parameter(typeof(T), "source");
            var block = new List<Expression>();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite || p.SetMethod is not null);

            foreach (var prop in properties)
            {
                var targetValue = Expression.Property(target, prop);
                var sourceValue = Expression.Property(source, prop);

                // If target value is default, assign source value
                var isDefault = Expression.Equal(
                    Expression.Convert(targetValue, typeof(object)),
                    Expression.Constant(null, typeof(object)));

                if (prop.PropertyType.IsValueType)
                {
                    var defaultValue = Expression.Constant(Activator.CreateInstance(prop.PropertyType), prop.PropertyType);
                    isDefault = Expression.Equal(targetValue, defaultValue);
                }

                var assign = Expression.Assign(targetValue, sourceValue);
                var ifDefaultThenAssign = Expression.IfThen(isDefault, assign);
                block.Add(ifDefaultThenAssign);
            }

            return Expression.Lambda<Action<T, T>>(Expression.Block(block), target, source);
        });

        merger(local, @global);
        return local;
    }

}
