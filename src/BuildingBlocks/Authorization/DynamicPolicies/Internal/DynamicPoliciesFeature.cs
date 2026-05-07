using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

/// <summary>
/// Marker class for the DynamicPolicies feature.
/// </summary>
[ExcludeFromCodeCoverage]
[VKFeatureMarker(DynamicPoliciesConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class DynamicPoliciesFeature;

