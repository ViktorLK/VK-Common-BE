using System;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Context.Internal;

/// <summary>
/// Provides high-performance metadata access for multi-tenancy operations.
/// </summary>
internal static class VKMultiTenancyTypeMetadata
{
    /// <summary>
    /// Checks if the specified type implements the multi-tenant interface.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is multi-tenant; otherwise, false.</returns>
    public static bool IsMultiTenant(Type type) => VKEntityMetadata.IsMultiTenant(type);
}
