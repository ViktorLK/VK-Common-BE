using VK.Blocks.Core;

namespace VK.Blocks.Validation;

/// <summary>
/// A marker type for the VK.Blocks.Validation building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKValidationBlock : IVKBlockMarker;
