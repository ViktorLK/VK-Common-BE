using System;
using System.Collections.Generic;
using VK.Blocks.Core.Constants;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Core.Contracts;

/// <summary>
/// A marker type for the VK.Blocks.Core building block.
/// Required to satisfy Rule 13 dependency checks in other modules.
/// </summary>
public sealed class CoreBlock : IVKBlockMarker
{
    private const string _identifier = "Core";

    /// <inheritdoc />
    public static string Identifier => _identifier;

    /// <inheritdoc />
    public static string Version => "0.9.0";

    /// <inheritdoc />
    public static IReadOnlyList<Type> Dependencies => [];

    /// <inheritdoc />
    public static string ActivitySourceName => VKBlocksConstants.VKBlocksPrefix + _identifier;

    /// <inheritdoc />
    public static string MeterName => VKBlocksConstants.VKBlocksPrefix + _identifier;
}


