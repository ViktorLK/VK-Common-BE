using System.Collections.Generic;
using System.Net;
using VK.Blocks.Authorization.InternalNetwork.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Internal Network authorization feature.
/// </summary>

public sealed partial record VKInternalNetworkOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the internal network feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the list of allowed CIDR ranges for internal network policies.
    /// </summary>
    [VKRequestOverride]
    public IReadOnlyList<string> InternalCidrs { get; init; } = InternalNetworkConstants.DefaultPrivateCidrs;

    /// <summary>Request-specific override for RemoteIp.</summary>
    [VKRequestOverride]
    public IPAddress? RemoteIp { get; init; }
}
