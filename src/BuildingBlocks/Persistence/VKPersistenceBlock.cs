using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// A marker type for the VK.Blocks.Persistence building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKPersistenceBlock;
