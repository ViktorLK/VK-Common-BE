using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect;

/// <summary>
/// A marker type for the VK.Blocks.Authentication.OpenIdConnect building block.
/// Complies with BB.02 (IVKBlockMarker).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution; contains no executable logic.")]
[VKBlockMarker(Dependencies = [typeof(VKAuthenticationBlock)])]
public sealed partial class VKOidcBlock;

