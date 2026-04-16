using System.Collections.Concurrent;
using VK.Blocks.Core.Primitives;

namespace VK.Blocks.Core.Caches;

/// <summary>
/// Bit-flags representing the capabilities and interfaces implemented by an entity type.
/// </summary>
[Flags]
internal enum EntityCapability : byte
{
    None = 0,
    Auditable = 1,
    SoftDelete = 2,
    MultiTenant = 4,
    MultiTenantEntity = 8
}

/// <summary>
/// Provides high-performance, non-generic access to entity metadata indicators (Rule 4).
/// Uses a unified BitFlags dictionary to minimize hash lookups in hot paths like EFCore lifecycle processing.
/// </summary>
public static class EntityMetadata
{
    private static readonly ConcurrentDictionary<Type, EntityCapability> CapabilityCache = new();

    /// <summary>
    /// Checks if a type implements <see cref="IAuditable"/>.
    /// </summary>
    public static bool IsAuditable(Type type) => (GetCapabilities(type) & EntityCapability.Auditable) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="ISoftDelete"/>.
    /// </summary>
    public static bool IsSoftDelete(Type type) => (GetCapabilities(type) & EntityCapability.SoftDelete) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IMultiTenant"/>.
    /// </summary>
    public static bool IsMultiTenant(Type type) => (GetCapabilities(type) & EntityCapability.MultiTenant) != 0;

    /// <summary>
    /// Checks if a type implements <see cref="IMultiTenantEntity"/>.
    /// </summary>
    public static bool IsMultiTenantEntity(Type type) => (GetCapabilities(type) & EntityCapability.MultiTenantEntity) != 0;

    /// <summary>
    /// Checks if the type is assignable to the target type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="targetType">The target type to check against.</param>
    /// <returns>True if assignable, otherwise false.</returns>
    public static bool IsAssignableTo(Type type, Type targetType) => type.IsAssignableTo(targetType);

    private static EntityCapability GetCapabilities(Type type)
    {
        return CapabilityCache.GetOrAdd(type, t =>
        {
            var cap = EntityCapability.None;

            if (typeof(IAuditable).IsAssignableFrom(t)) cap |= EntityCapability.Auditable;
            if (typeof(ISoftDelete).IsAssignableFrom(t)) cap |= EntityCapability.SoftDelete;
            if (typeof(IMultiTenant).IsAssignableFrom(t)) cap |= EntityCapability.MultiTenant;
            if (typeof(IMultiTenantEntity).IsAssignableFrom(t)) cap |= EntityCapability.MultiTenantEntity;

            return cap;
        });
    }
}
