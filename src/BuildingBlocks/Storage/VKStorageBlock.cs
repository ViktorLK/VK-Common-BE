using VK.Blocks.Core;

namespace VK.Blocks.Storage;

/// <summary>
/// A marker type for the VK.Blocks.Storage building block.
/// </summary>
[VKBlockMarker("Storage", Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKStorageBlock;
