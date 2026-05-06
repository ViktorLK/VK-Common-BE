using System;
using VK.Blocks.Authentication.OpenIdConnect.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect;

/// <summary>
/// Extension methods for configuring OpenIdConnect authentication block.
/// Complies with Rule 18.1 (Public Wrapper).
/// </summary>
public static class VKOidcBlockExtensions
{
    public static IVKBlockBuilder<VKAuthenticationBlock> AddVKOidcBlock(
        this IVKBlockBuilder<VKAuthenticationBlock> builder)
    {
        VKGuard.NotNull(builder);
        return OidcBlockRegistration.Register(builder, builder.Configuration);
    }

    /// <summary>
    /// Adds OIDC block to the authentication pipeline with manual options configuration.
    /// Following ADR-016: Use 'with' expression to modify immutable options.
    /// </summary>
    /// <param name="builder">The authentication block builder.</param>
    /// <param name="configure">The options transformation function.</param>
    /// <returns>The same builder instance.</returns>
    public static IVKBlockBuilder<VKAuthenticationBlock> AddVKOidcBlock(
        this IVKBlockBuilder<VKAuthenticationBlock> builder,
        Func<VKOidcOptions, VKOidcOptions> configure)
    {
        VKGuard.NotNull(builder);
        return OidcBlockRegistration.Register(builder, builder.Configuration, configure);
    }
}
