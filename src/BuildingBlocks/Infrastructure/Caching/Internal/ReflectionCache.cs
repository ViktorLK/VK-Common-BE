using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace VK.Blocks.Infrastructure.Caching.Internal;

/// <summary>
/// A high-performance, thread-safe cache for reflection metadata.
/// Reduces the cost of repeatedly looking up types and members.
/// </summary>
internal static class ReflectionCache
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _properties = new();
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> _propertyByName = new();

    /// <summary>
    /// Gets all public instance properties for a type, with caching.
    /// </summary>
    public static PropertyInfo[] GetProperties(Type type)
    {
        return _properties.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
    }

    /// <summary>
    /// Gets a specific property by name with caching.
    /// </summary>
    public static PropertyInfo? GetProperty(Type type, string propertyName)
    {
        return _propertyByName.GetOrAdd((type, propertyName), key =>
            key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase));
    }
}
