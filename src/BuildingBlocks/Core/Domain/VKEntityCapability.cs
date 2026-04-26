using System;

namespace VK.Blocks.Core;

/// <summary>
/// Bit-flags representing the capabilities and interfaces implemented by an entity type.
/// </summary>
[Flags]
public enum VKEntityCapability : byte
{
    None = 0,
    Auditable = 1,
    SoftDelete = 2,
    MultiTenant = 4,
    MultiTenantEntity = 8
}
