using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;
using VK.Blocks.ExceptionHandling;

namespace VK.Blocks.Web;

/// <summary>
/// A marker type for the VK.Blocks.Web building block.
/// Complies with BB.02 (IVKBlockMarker).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution; contains no executable logic.")]
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock), typeof(VKExceptionHandlingBlock)])]
public sealed partial class VKWebBlock;
