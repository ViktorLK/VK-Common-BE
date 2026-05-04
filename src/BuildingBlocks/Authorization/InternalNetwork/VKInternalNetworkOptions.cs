using System.Collections.Generic;
using VK.Blocks.Authorization.InternalNetwork.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Internal Network authorization feature.
/// </summary>
public sealed record VKInternalNetworkOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAuthorizationBlock.BlockName}:{InternalNetworkConstants.FeatureName}";

    /// <summary>
    /// Gets a value indicating whether the internal network feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the list of allowed CIDR ranges for internal network policies.
    /// </summary>
    public IReadOnlyList<string> InternalCidrs { get; init; } = InternalNetworkConstants.DefaultPrivateCidrs;
}
