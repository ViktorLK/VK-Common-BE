using System;

namespace VK.Blocks.Core.Reflection.Internal;

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
