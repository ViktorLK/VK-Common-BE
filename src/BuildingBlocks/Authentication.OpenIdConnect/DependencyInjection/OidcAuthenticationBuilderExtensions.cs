using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core.DependencyInjection;
using VK.Blocks.Core.Diagnostics;
using VK.Blocks.Authentication.DependencyInjection;
using VK.Blocks.Authentication.OpenIdConnect.Diagnostics;
using VK.Blocks.Authentication.OpenIdConnect.Features.Oidc;
using VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;

namespace VK.Blocks.Authentication.OpenIdConnect.DependencyInjection;

/// <summary>
/// Extension methods for configuring OpenId Connect authentication.
/// </summary>
public static class OidcAuthenticationBuilderExtensions
{
    /// <summary>
    /// Discovers and registers OAuth providers with Fail-Fast validation.
    /// </summary>
    /// <param name="builder">The VK block builder.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <returns>The same <paramref name="builder"/> instance for chaining.</returns>
    public static IVKBlockBuilder<AuthenticationBlock> AddVKOidcBlock(this IVKBlockBuilder<AuthenticationBlock> builder, IConfiguration configuration)
    {
        // 0. Idempotency Check
        if (builder.Services.IsVKBlockRegistered<OidcBlock>())
        {
            return builder;
        }

        // b) Ensure the core Authentication block is registered as a prerequisite.
        if (!builder.Services.IsVKBlockRegistered<AuthenticationBlock>())
        {
            throw new InvalidOperationException(OidcConstants.DependencyMissingMessage);
        }

        // 1. Feature Registration
        builder.Services.AddOidcFeature(configuration);

        // 1.5 Diagnostic Registration
        builder.Services.TryAddEnumerableSingleton<ISecurityMetadataProvider, OidcMetadataProvider>();

        // 2. Mark-Self (Success Commit)
        builder.Services.AddVKBlockMarker<OidcBlock>();

        return builder;
    }
}
