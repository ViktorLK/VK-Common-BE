using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Architectural Marker for VK.Blocks.AI.Cortex BuildingBlock.
/// Governed by [BB.02] Marker Pattern &amp; [BB.03] Dependency declarations.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKAIPsycheBlock)])]
public sealed partial class VKAICortexBlock;
