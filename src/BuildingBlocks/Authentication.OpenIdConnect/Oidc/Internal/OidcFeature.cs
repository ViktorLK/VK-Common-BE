using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect.Oidc.Internal;

/// <summary>
/// Feature marker class for the OpenID Connect (OIDC) authentication feature.
/// This allows the system to identify and track OIDC-related logic and telemetry.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Feature marker class used for telemetry identification; contains no executable logic.")]
[VKFeatureMarker(OidcConstants.FeatureName, typeof(VKOidcBlock))]
internal sealed partial class OidcFeature;
