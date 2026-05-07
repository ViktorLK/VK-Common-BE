using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Entitlements.Internal;

/// <summary>
/// Marker class for the Entitlements feature.
/// </summary>
[ExcludeFromCodeCoverage]
[VKFeatureMarker(EntitlementsConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class EntitlementsFeature;

