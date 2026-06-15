using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// AI.Engram Block Marker.
/// Follows BB.02.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKAIBlock)])]
public sealed partial class VKAIEngramBlock;
