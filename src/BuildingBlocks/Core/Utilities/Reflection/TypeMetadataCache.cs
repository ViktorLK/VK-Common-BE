using System;
using System.Linq;
using VK.Blocks.Core.Domain;

namespace VK.Blocks.Core.Utilities.Reflection;

/// <summary>
/// Cache for entity type capabilities and metadata.
/// </summary>
/// <typeparam name="T">The type to cache metadata for.</typeparam>
public sealed class TypeMetadataCache<T>
{
    private TypeMetadataCache()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the type implements <see cref="IAuditable"/>.
    /// </summary>
    public static readonly bool IsAuditable = typeof(IAuditable).IsAssignableFrom(typeof(T));

    /// <summary>
    /// Gets a value indicating whether the type implements <see cref="ISoftDelete"/>.
    /// </summary>
    public static readonly bool IsSoftDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(T));

    /// <summary>
    /// Gets a value indicating whether the type implements <see cref="IMultiTenant"/>.
    /// </summary>
    public static readonly bool IsMultiTenant = typeof(IMultiTenant).IsAssignableFrom(typeof(T));

    /// <summary>
    /// Gets the name of the type.
    /// </summary>
    public static readonly string Name = typeof(T).Name;

    /// <summary>
    /// Gets the full name of the type.
    /// </summary>
    public static readonly string FullName = typeof(T).FullName ?? typeof(T).Name;

    /// <summary>
    /// Checks if the type is assignable to the specified interface or base class.
    /// </summary>
    /// <typeparam name="TTarget">The target type to check.</typeparam>
    /// <returns>True if assignable; otherwise, false.</returns>
    public static bool IsAssignableTo<TTarget>() => TypeRelationshipCache<T, TTarget>.IsAssignable;

    /// <summary>
    /// Gets a pre-cached attribute from the type.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type.</typeparam>
    /// <returns>The attribute instance or null.</returns>
    public static TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute
        => AttributeCache<T, TAttribute>.Attribute;

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

