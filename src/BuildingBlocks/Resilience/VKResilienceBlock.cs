using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// A marker type for the VK.Blocks.Resilience building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKResilienceBlock;
