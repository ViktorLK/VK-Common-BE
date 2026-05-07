using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.MinimumRank.Internal;

/// <summary>
/// Marker class for the MinimumRank feature.
/// </summary>
[ExcludeFromCodeCoverage]
[VKFeatureMarker(MinimumRankConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class MinimumRankFeature;

