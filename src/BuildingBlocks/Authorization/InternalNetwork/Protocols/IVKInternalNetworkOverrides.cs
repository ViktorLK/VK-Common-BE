using System.Collections.Generic;
using System.Net;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines request-level overrides and target parameters for Internal Network authorization.
/// </summary>
public interface IVKInternalNetworkOverrides
{
    /// <summary>
    /// Gets the list of allowed CIDR ranges for internal network policies, overriding the default settings.
    /// </summary>
    IReadOnlyList<string>? InternalCidrs { get; init; }

    /// <summary>
    /// Gets the explicit IP to check.
    /// </summary>
    IPAddress? RemoteIp { get; init; }
}
