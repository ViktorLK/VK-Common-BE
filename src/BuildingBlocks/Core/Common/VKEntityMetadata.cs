using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace VK.Blocks.Core;

/// <summary>
/// Provides high-performance, non-generic access to entity metadata indicators (Rule 4).
/// Uses a unified BitFlags dictionary to minimize hash lookups in hot paths like EFCore lifecycle processing.
/// </summary>
public static class VKEntityMetadata
{
    private static readonly ConcurrentDictionary<Type, VKEntityCapability> _capabilityCache = new();

    /// <summary>
    /// Checks if a type implements <see cref="IVKAuditable"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAuditable(Type type) => (GetCapabilities(type) & VKEntityCapability.Auditable) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IVKSoftDelete"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSoftDelete(Type type) => (GetCapabilities(type) & VKEntityCapability.SoftDelete) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IVKMultiTenant"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMultiTenant(Type type) => (GetCapabilities(type) & VKEntityCapability.MultiTenant) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IVKMultiTenantEntity"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMultiTenantEntity(Type type) => (GetCapabilities(type) & VKEntityCapability.MultiTenantEntity) != 0;

    /// <summary>
    /// Checks if the type is assignable to the target type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="targetType">The target type to check against.</param>
    /// <returns>True if assignable, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAssignableTo(Type type, Type targetType) => type.IsAssignableTo(targetType);

    private static VKEntityCapability GetCapabilities(Type type)
    {
        return _capabilityCache.GetOrAdd(type, t =>
        {
            VKEntityCapability cap = VKEntityCapability.None;

            if (typeof(IVKAuditable).IsAssignableFrom(t))
            {
                cap |= VKEntityCapability.Auditable;
            }

            if (typeof(IVKSoftDelete).IsAssignableFrom(t))
            {
                cap |= VKEntityCapability.SoftDelete;
            }

            if (typeof(IVKMultiTenant).IsAssignableFrom(t))
            {
                cap |= VKEntityCapability.MultiTenant;
            }

            if (typeof(IVKMultiTenantEntity).IsAssignableFrom(t))
            {
                cap |= VKEntityCapability.MultiTenantEntity;
            }

            return cap;
        });
    }
}
