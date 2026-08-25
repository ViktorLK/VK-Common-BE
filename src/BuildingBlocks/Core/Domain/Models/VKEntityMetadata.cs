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
    /// Checks if a type implements <see cref="IVKCreationAudited"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCreationAudited(Type type) => (GetCapabilities(type) & VKEntityCapability.CreationAudited) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IVKModificationAudited"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsModificationAudited(Type type) => (GetCapabilities(type) & VKEntityCapability.ModificationAudited) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IVKAuditable"/> (both creation and modification audited).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAuditable(Type type) => (GetCapabilities(type) & VKEntityCapability.Auditable) == VKEntityCapability.Auditable;

    /// <summary>
    /// Checks if a type implements <see cref="IVKSoftDeletable"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSoftDelete(Type type) => (GetCapabilities(type) & VKEntityCapability.SoftDeletable) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IVKDeletionAudited"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDeletionAudited(Type type) => (GetCapabilities(type) & VKEntityCapability.DeletionAudited) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IVKTenantScoped"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMultiTenant(Type type) => (GetCapabilities(type) & VKEntityCapability.MultiTenant) != 0;

    /// <summary>
    /// Checks if the type is assignable to the target type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="targetType">The target type to check against.</param>
    /// <returns>True if assignable; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAssignableTo(Type type, Type targetType) => targetType.IsAssignableFrom(type);

    /// <summary>
    /// Gets the aggregate capabilities bitmask for a given type.
    /// Uses lock-free immutable replacement for atomic and thread-safe updates.
    /// </summary>
    /// <param name="type">The entity type to check.</param>
    /// <returns>A bitmask of <see cref="VKEntityCapability"/> flags.</returns>
    public static VKEntityCapability GetCapabilities(Type type)
    {
        VKGuard.NotNull(type);

        if (_capabilityCache.TryGetValue(type, out var caps))
        {
            return caps;
        }

        var newCaps = ComputeCapabilities(type);

        lock (_syncLock)
        {
            if (!_capabilityCache.ContainsKey(type))
            {
                var dictionary = new Dictionary<Type, VKEntityCapability>(_capabilityCache)
                {
                    [type] = newCaps
                };
                _capabilityCache = dictionary.ToFrozenDictionary();
            }
        }

        return newCaps;
    }

    private static VKEntityCapability ComputeCapabilities(Type t)
    {
        VKEntityCapability cap = VKEntityCapability.None;

        if (typeof(IVKCreationAudited).IsAssignableFrom(t))
        {
            cap |= VKEntityCapability.CreationAudited;
        }

        if (typeof(IVKModificationAudited).IsAssignableFrom(t))
        {
            cap |= VKEntityCapability.ModificationAudited;
        }

        if (typeof(IVKSoftDeletable).IsAssignableFrom(t))
        {
            cap |= VKEntityCapability.SoftDeletable;
        }

        if (typeof(IVKDeletionAudited).IsAssignableFrom(t))
        {
            cap |= VKEntityCapability.DeletionAudited;
        }

        if (typeof(IVKTenantScoped).IsAssignableFrom(t))
        {
            cap |= VKEntityCapability.MultiTenant;
        }

        return cap;
    }
}
