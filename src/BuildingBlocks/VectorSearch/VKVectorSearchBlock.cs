using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// AI.Recall Block Marker.
/// Follows BB.02.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKVectorStoreBlock)])]
public sealed partial class VKVectorSearchBlock;
