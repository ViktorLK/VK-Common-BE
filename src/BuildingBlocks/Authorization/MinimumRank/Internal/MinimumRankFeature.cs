using VK.Blocks.Core;

namespace VK.Blocks.Authorization.MinimumRank.Internal;

/// <summary>
/// Marker class for the MinimumRank feature.
/// </summary>
[VKFeatureMarker(MinimumRankConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class MinimumRankFeature;
