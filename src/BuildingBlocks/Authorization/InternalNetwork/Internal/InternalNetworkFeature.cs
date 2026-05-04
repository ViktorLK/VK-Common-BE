using VK.Blocks.Core;

namespace VK.Blocks.Authorization.InternalNetwork.Internal;

/// <summary>
/// Marker class for the InternalNetwork feature.
/// </summary>
[VKFeatureMarker(InternalNetworkConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class InternalNetworkFeature;
