using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Permissions.Internal;

/// <summary>
/// Marker class for the Permissions feature.
/// </summary>
[ExcludeFromCodeCoverage]
[VKFeatureMarker(PermissionsConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class PermissionsFeature;

