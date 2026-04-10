using System.Collections.Generic;
using VK.Blocks.Authorization.Abstractions;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.InternalNetwork;

/// <summary>
/// Requires the caller's IP address to fall within one of the configured CIDR ranges.
/// </summary>
/// <param name="AllowedCidrs">Allowed CIDR notations.</param>
public sealed record InternalNetworkRequirement(IReadOnlyList<string> AllowedCidrs) : IVKAuthorizationRequirement
{
    /// <inheritdoc />
    public Error DefaultError => AuthorizationErrors.InternalNetworkDenied;
}
