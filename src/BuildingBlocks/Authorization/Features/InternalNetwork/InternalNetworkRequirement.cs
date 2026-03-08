using System.Collections.Generic;

namespace VK.Blocks.Authorization.Features.InternalNetwork;

/// <summary>
/// Requires the caller's IP address to fall within one of the configured CIDR ranges.
/// Use with <c>InternalNetworkAuthorizationHandler</c>.
/// </summary>
/// <param name="AllowedCidrs">
///     Allowed CIDR notations, e.g. <c>["10.0.0.0/8", "192.168.1.0/24"]</c>.
///     Supports both IPv4 and IPv6.
/// </param>
public sealed record InternalNetworkRequirement : Microsoft.AspNetCore.Authorization.IAuthorizationRequirement
{
    #region Properties

    /// <summary>
    /// Allowed CIDR notations, e.g. <c>["10.0.0.0/8", "192.168.1.0/24"]</c>.
    /// Supports both IPv4 and IPv6.
    /// </summary>
    public IReadOnlyList<string> AllowedCidrs { get; init; }

    #endregion

    public InternalNetworkRequirement(IReadOnlyList<string> allowedCidrs)
    {
        AllowedCidrs = allowedCidrs;
    }
}


