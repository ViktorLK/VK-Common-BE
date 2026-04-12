using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authentication.DependencyInjection;
using VK.Blocks.Authentication.OpenIdConnect.Features.Oidc;
using VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.OpenIdConnect.DependencyInjection;

/// <summary>
/// Extension methods for configuring OpenId Connect authentication.
/// </summary>
public static class OidcAuthenticationBuilderExtensions
{
    #region Public Methods

    /// <summary>
    /// Discovers and registers OAuth providers with Fail-Fast validation.
    /// </summary>
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

        // 2. Mark-Self (Success Commit)
        builder.Services.AddVKBlockMarker<OidcBlock>();

        return builder;
    }

    #endregion
}
