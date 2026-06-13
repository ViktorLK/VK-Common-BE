using VK.Blocks.Core;

namespace VK.Blocks.AI.Recall;

/// <summary>
/// AI.Recall Block Marker.
/// Follows BB.02.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKAIBlock)])]
public sealed partial class VKRecallBlock;
