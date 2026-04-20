using System.Collections.Generic;
using VK.Blocks.Core.Constants;
using VK.Blocks.Core.DependencyInjection;

namespace VK.Blocks.Core.Contracts;

/// <summary>
/// A marker type for the VK.Blocks.Core building block.
/// Required to satisfy Rule 13 dependency checks in other modules.
/// </summary>
public sealed partial class CoreBlock : IVKBlockMarker
{
    private CoreBlock() { }

    /// <inheritdoc />
    public string Identifier => "Core";

    /// <inheritdoc />
    public string Version => "0.9.0";

    /// <inheritdoc />
    public IReadOnlyList<IVKBlockMarker> Dependencies => [];

    /// <inheritdoc />
    public string ActivitySourceName => VKBlocksConstants.VKBlocksPrefix + Identifier;

    /// <inheritdoc />
    public string MeterName => VKBlocksConstants.VKBlocksPrefix + Identifier;
}
