using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// AI.Psyche Block Marker.
/// Follows BB.02.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKAIBlock)], Toggleable = false)]
public sealed partial class VKAIPsycheBlock;
