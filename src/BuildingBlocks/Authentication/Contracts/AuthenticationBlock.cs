using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// A marker type for the VK.Blocks.Authentication building block.
/// </summary>
public sealed partial class AuthenticationBlock : IVKBlockMarker
{
    private AuthenticationBlock() { }

    /// <inheritdoc />
    public string Identifier => "Authentication";

    /// <inheritdoc />
    public string Version => "0.9.0";

    /// <inheritdoc />
    public IReadOnlyList<IVKBlockMarker> Dependencies => [VKCoreBlock.Instance];

    /// <inheritdoc />
    public string ActivitySourceName => VKBlocksConstants.VKBlocksPrefix + Identifier;

    /// <inheritdoc />
    public string MeterName => VKBlocksConstants.VKBlocksPrefix + Identifier;
}
