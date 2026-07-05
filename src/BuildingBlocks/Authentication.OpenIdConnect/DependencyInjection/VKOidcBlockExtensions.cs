using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authentication.OpenIdConnect.Common.DependencyInjection.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect;

/// <summary>
/// Fluent extensions for adding OIDC support to the Authentication block.
/// Complies with Level 1 Public API pattern (AP.03).
/// </summary>
[ExcludeFromCodeCoverage]
public static partial class VKOidcBlockExtensions
{
    /// <summary>
    /// Adds OIDC block to the authentication pipeline.
    /// </summary>
    public static IVKBlockBuilder<VKAuthenticationBlock> AddVKOidcBlock(
        this IVKBlockBuilder<VKAuthenticationBlock> builder)
    {
        VKGuard.NotNull(builder);
        _ = OidcBlockRegistration.Register(builder.Services, builder.Configuration);
        return builder;
    }

    /// <summary>
    /// Adds OIDC block to the authentication pipeline with manual options configuration.
    /// </summary>
    public static IVKBlockBuilder<VKAuthenticationBlock> AddVKOidcBlock(
        this IVKBlockBuilder<VKAuthenticationBlock> builder,
        Func<VKOidcOptions, VKOidcOptions> configure)
    {
        VKGuard.NotNull(builder);
        _ = OidcBlockRegistration.Register(builder.Services, builder.Configuration, configure);
        return builder;
    }
}
