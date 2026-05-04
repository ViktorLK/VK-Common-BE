using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

/// <summary>
/// Marker class for the DynamicPolicies feature.
/// </summary>
[VKFeatureMarker(DynamicPoliciesConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class DynamicPoliciesFeature;
