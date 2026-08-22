using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Messaging;

/// <summary>
/// A marker type for the VK.Blocks.Messaging building block.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKMessagingBlock;
