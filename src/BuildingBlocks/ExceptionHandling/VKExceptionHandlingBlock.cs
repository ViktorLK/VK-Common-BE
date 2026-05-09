using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// A marker type for the VK.Blocks.ExceptionHandling building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKExceptionHandlingBlock;
