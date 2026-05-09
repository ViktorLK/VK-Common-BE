using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Observability;

/// <summary>
/// A marker type for the VK.Blocks.Observability building block.
/// Complies with BB.02.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution; contains no executable logic.")]
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKObservabilityBlock;

