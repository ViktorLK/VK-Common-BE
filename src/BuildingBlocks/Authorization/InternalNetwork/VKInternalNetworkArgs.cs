using System.Collections.Generic;
using System.Net;

using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Arguments for internal network evaluation.
/// Following AP.05: Local overrides for the global <see cref="VKInternalNetworkOptions"/>.
/// </summary>
public sealed record VKInternalNetworkArgs : IVKArgs<VKInternalNetworkArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKInternalNetworkArgs Empty { get; } = new();

    /// <summary>
    /// Gets the explicit IP to check.
    /// If null, the implementation should resolve it automatically (e.g. from context).
    /// </summary>
    public IPAddress? RemoteIp { get; init; }

    /// <summary>
    /// Gets the list of allowed CIDR ranges.
    /// If null, the value from global options is used.
    /// </summary>
    public IReadOnlyList<string>? AllowedCidrs { get; init; }
}

