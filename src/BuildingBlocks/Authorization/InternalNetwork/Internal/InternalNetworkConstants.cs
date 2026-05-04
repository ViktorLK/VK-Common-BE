using System.Collections.Generic;

namespace VK.Blocks.Authorization.InternalNetwork.Internal;

/// <summary>
/// Defines constants and default values for the InternalNetwork feature.
/// </summary>
internal static class InternalNetworkConstants
{
    /// <summary>
    /// The standard RFC 1918 private IPv4 ranges plus loopback.
    /// </summary>
    internal static readonly IReadOnlyList<string> DefaultPrivateCidrs =
    [
        "10.0.0.0/8",
        "172.16.0.0/12",
        "192.168.0.0/16",
        "127.0.0.1/32"
    ];

    /// <summary>
    /// The name of the InternalNetwork feature.
    /// </summary>
    internal const string FeatureName = "InternalNetwork";
}
