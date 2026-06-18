using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// A marker type for the VK.Blocks.Persistence.Cosmos building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKPersistenceBlock)])]
public sealed partial class VKCosmosPersistenceBlock;
