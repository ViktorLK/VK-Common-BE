using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using VK.Blocks.Core.Reflection.Internal;

namespace VK.Blocks.Core;

/// <summary>
/// Provides high-performance, non-generic access to entity metadata indicators (Rule 4).
/// Uses a unified BitFlags dictionary to minimize hash lookups in hot paths like EFCore lifecycle processing.
/// </summary>
public static class VKEntityMetadata
{
    private static readonly ConcurrentDictionary<Type, EntityCapability> _capabilityCache = new();

    /// <summary>
    /// Checks if a type implements <see cref="IVKAuditable"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAuditable(Type type) => (GetCapabilities(type) & EntityCapability.Auditable) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IVKSoftDelete"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSoftDelete(Type type) => (GetCapabilities(type) & EntityCapability.SoftDelete) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IVKMultiTenant"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMultiTenant(Type type) => (GetCapabilities(type) & EntityCapability.MultiTenant) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IVKMultiTenantEntity"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMultiTenantEntity(Type type) => (GetCapabilities(type) & EntityCapability.MultiTenantEntity) != 0;

    /// <summary>
    /// Checks if the type is assignable to the target type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="targetType">The target type to check against.</param>
    /// <returns>True if assignable, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAssignableTo(Type type, Type targetType) => type.IsAssignableTo(targetType);

    private static EntityCapability GetCapabilities(Type type)
    {
        return _capabilityCache.GetOrAdd(type, t =>
        {
            EntityCapability cap = EntityCapability.None;

            if (typeof(IVKAuditable).IsAssignableFrom(t))
            {
                cap |= EntityCapability.Auditable;
            }

            if (typeof(IVKSoftDelete).IsAssignableFrom(t))
            {
                cap |= EntityCapability.SoftDelete;
            }

            if (typeof(IVKMultiTenant).IsAssignableFrom(t))
            {
                cap |= EntityCapability.MultiTenant;
            }

            if (typeof(IVKMultiTenantEntity).IsAssignableFrom(t))
            {
                cap |= EntityCapability.MultiTenantEntity;
            }

            return cap;
        });
    }
}
