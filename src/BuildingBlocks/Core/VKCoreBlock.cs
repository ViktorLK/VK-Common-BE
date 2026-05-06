using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Core;

/// <summary>
/// A marker type for the VK.Blocks.Core building block.
/// Required to satisfy Rule 13 dependency checks in other modules.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker]
public sealed partial class VKCoreBlock;
