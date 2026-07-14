using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// A marker type for the VK.Blocks.Persistence.EFCore building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKPersistenceEFCoreBlock;
