using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// A marker type for the VK.Blocks.Authentication building block.
/// </summary>
[ExcludeFromCodeCoverage]
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKAuthenticationBlock;
