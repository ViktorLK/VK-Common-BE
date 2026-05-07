using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.InternalNetwork.Internal;

/// <summary>
/// Marker class for the InternalNetwork feature.
/// </summary>
[ExcludeFromCodeCoverage]
[VKFeatureMarker(InternalNetworkConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class InternalNetworkFeature;

