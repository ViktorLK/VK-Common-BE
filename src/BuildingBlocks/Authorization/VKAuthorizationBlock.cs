using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// A marker type for the VK.Blocks.Authorization building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKAuthorizationBlock
{
}
