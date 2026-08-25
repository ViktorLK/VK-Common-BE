using System;

namespace VK.Blocks.Core;

/// <summary>
/// Bit-flags representing the capabilities and interfaces implemented by an entity type.
/// </summary>
[Flags]
public enum VKEntityCapability : ushort
{
    None = 0,
    CreationAudited = 1 << 0,     // 1
    ModificationAudited = 1 << 1, // 2
    SoftDeletable = 1 << 2,       // 4
    DeletionAudited = 1 << 3,     // 8
    MultiTenant = 1 << 4,         // 16

    // Composite Shortcuts
    Auditable = CreationAudited | ModificationAudited,
    SoftDeleteAudited = SoftDeletable | DeletionAudited
}
