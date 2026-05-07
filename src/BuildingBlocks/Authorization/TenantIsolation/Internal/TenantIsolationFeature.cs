using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.TenantIsolation.Internal;

/// <summary>
/// Marker class for the TenantIsolation feature.
/// </summary>
[ExcludeFromCodeCoverage]
[VKFeatureMarker(TenantIsolationConstants.FeatureName, typeof(VKAuthorizationBlock))]
internal sealed partial class TenantIsolationFeature;

