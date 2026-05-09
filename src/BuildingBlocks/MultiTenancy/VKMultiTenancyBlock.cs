using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// A marker type for the VK.Blocks.MultiTenancy building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKMultiTenancyBlock;
