using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Roles.Internal;

/// <summary>
/// Marker class for the Roles feature.
/// </summary>
[VKFeatureMarker(RolesConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class RolesFeature;
