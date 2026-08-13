using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VK.Blocks.Core;

/// <summary>
/// Provides high-performance, non-generic access to entity metadata indicators (CS.04).
/// Uses a unified BitFlags FrozenDictionary to minimize hash lookups in hot paths like EFCore lifecycle processing.
/// </summary>
public static class VKEntityMetadata
{
    private static readonly object _syncLock = new();
    private static FrozenDictionary<Type, VKEntityCapability> _capabilityCache = FrozenDictionary<Type, VKEntityCapability>.Empty;

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
        if (_capabilityCache.TryGetValue(type, out var capability))
        {
            return capability;
        }

        lock (_syncLock)
        {
            if (_capabilityCache.TryGetValue(type, out capability))
            {
                return capability;
            }

            var builder = new Dictionary<Type, VKEntityCapability>(_capabilityCache);
            builder[type] = ComputeCapabilities(type);
            _capabilityCache = builder.ToFrozenDictionary();
            return _capabilityCache[type];
        }
    }

    private static VKEntityCapability ComputeCapabilities(Type t)
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
    }
}
