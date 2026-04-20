using System;
using System.Collections.Generic;
using VK.Blocks.Core.Constants;
using VK.Blocks.Core.Contracts;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Authorization.Contracts;

/// <summary>
/// A marker type for the VK.Blocks.Authorization building block.
/// </summary>
public sealed class AuthorizationBlock : IVKBlockMarker
{
    private const string _identifier = "Authorization";

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


