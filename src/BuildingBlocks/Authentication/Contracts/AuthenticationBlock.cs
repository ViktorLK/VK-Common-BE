using System;
using System.Collections.Generic;
using VK.Blocks.Core.Constants;
using VK.Blocks.Core.Contracts;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authentication.Contracts;

/// <summary>
/// A marker type for the VK.Blocks.Authentication building block.
/// </summary>
public sealed class AuthenticationBlock : IVKBlockMarker
{
    private const string _identifier = "Authentication";

    /// <inheritdoc />
    public static string Identifier => _identifier;

    /// <inheritdoc />
    public static string Version => "0.9.0";

    /// <inheritdoc />
    public static IReadOnlyList<Type> Dependencies => [typeof(CoreBlock)];

    /// <inheritdoc />
    public static string ActivitySourceName => VKBlocksConstants.VKBlocksPrefix + _identifier;

    /// <inheritdoc />
    public static string MeterName => VKBlocksConstants.VKBlocksPrefix + _identifier;
}


