using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Cosmos;

/// <summary>
/// A marker type for the VK.Blocks.Persistence.Cosmos building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKPersistenceBlock)])]
public sealed partial class VKPersistenceEFCoreCosmosBlock;
