using System.Runtime.CompilerServices;

namespace VK.Blocks.Persistence.EFCore.Interceptors.Internal;

/// <summary>
/// Thread-safe registry to flag entities that should be physically deleted, bypassing soft-delete interceptors.
/// </summary>
internal static class PhysicalDeleteRegistry
{
    private static readonly ConditionalWeakTable<object, object> Registry = new();

    /// <summary>
    /// Registers an entity to be physically deleted.
    /// </summary>
    public static void Register(object entity)
    {
        Registry.AddOrUpdate(entity, true);
    }

    /// <summary>
    /// Checks if the entity is registered for physical delete and unregisters it if found.
    /// </summary>
    public static bool ShouldPhysicalDelete(object entity)
    {
        if (Registry.TryGetValue(entity, out _))
        {
            Registry.Remove(entity);
            return true;
        }
        return false;
    }
}
