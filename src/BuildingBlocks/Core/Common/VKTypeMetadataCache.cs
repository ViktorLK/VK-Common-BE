using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace VK.Blocks.Core;

/// <summary>
/// Provides high-performance caching for type metadata and entity capabilities using Static Generic Caching.
/// </summary>
public sealed class VKTypeMetadataCache
{
    private VKTypeMetadataCache()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the type implements <see cref="IVKAuditable"/>.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAuditable<T>() => InnerCache<T>.IsAuditable;

    /// <summary>
    /// Gets a value indicating whether the type implements <see cref="IVKSoftDelete"/>.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSoftDelete<T>() => InnerCache<T>.IsSoftDelete;

    /// <summary>
    /// Gets a value indicating whether the type implements <see cref="IVKMultiTenant"/>.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMultiTenant<T>() => InnerCache<T>.IsMultiTenant;

    /// <summary>
    /// Gets the name of the type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName<T>() => InnerCache<T>.Name;

    /// <summary>
    /// Gets the full name of the type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetFullName<T>() => InnerCache<T>.FullName;

    /// <summary>
    /// Checks if the type is assignable to the specified interface or base class.
    /// </summary>
    /// <typeparam name="TSource">The source type to check.</typeparam>
    /// <typeparam name="TTarget">The target type to check against.</typeparam>
    /// <returns>True if assignable; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAssignableTo<TSource, TTarget>() => TypeRelationshipCache<TSource, TTarget>.IsAssignable;

    /// <summary>
    /// Gets a pre-cached attribute from the type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <typeparam name="TAttribute">The attribute type.</typeparam>
    /// <returns>The attribute instance or null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TAttribute? GetAttribute<T, TAttribute>() where TAttribute : Attribute
        => AttributeCache<T, TAttribute>.Attribute;

    private static class InnerCache<T>
    {
        public static readonly bool IsAuditable = typeof(IVKAuditable).IsAssignableFrom(typeof(T));
        public static readonly bool IsSoftDelete = typeof(IVKSoftDelete).IsAssignableFrom(typeof(T));
        public static readonly bool IsMultiTenant = typeof(IVKMultiTenant).IsAssignableFrom(typeof(T));
        public static readonly string Name = typeof(T).Name;
        public static readonly string FullName = typeof(T).FullName ?? typeof(T).Name;
    }

    private static class TypeRelationshipCache<TSource, TTarget>
    {
        public static readonly bool IsAssignable = typeof(TTarget).IsAssignableFrom(typeof(TSource));
    }

    private static class AttributeCache<TType, TAttribute> where TAttribute : Attribute
    {
        public static readonly TAttribute? Attribute = (TAttribute?)typeof(TType)
            .GetCustomAttributes(typeof(TAttribute), true)
            .FirstOrDefault();
    }
}
