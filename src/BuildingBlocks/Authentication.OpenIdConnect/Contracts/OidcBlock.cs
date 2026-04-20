using System;
using System.Collections.Generic;
using VK.Blocks.Authentication.Contracts;
using VK.Blocks.Core.Constants;
using VK.Blocks.Core.Contracts;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.OpenIdConnect.Contracts;

/// <summary>
/// A marker type for the VK.Blocks.Authentication.OpenIdConnect building block.
/// </summary>
public sealed class OidcBlock : IVKBlockMarker
{
    private const string _identifier = "Authentication.Oidc";

    /// <inheritdoc />
    public static string Identifier => _identifier;

    /// <inheritdoc />
    public static string Version => "0.9.0";

    /// <inheritdoc />
    public static IReadOnlyList<Type> Dependencies => [typeof(CoreBlock), typeof(AuthenticationBlock)];

    /// <inheritdoc />
    public static string ActivitySourceName => VKBlocksConstants.VKBlocksPrefix + _identifier;

    /// <inheritdoc />
    public static string MeterName => VKBlocksConstants.VKBlocksPrefix + _identifier;
}


