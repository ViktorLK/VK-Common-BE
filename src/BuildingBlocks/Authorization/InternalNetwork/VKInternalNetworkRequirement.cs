using System.Collections.Generic;


using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Requires the caller's IP address to fall within one of the configured CIDR ranges.
/// </summary>
/// <param name="AllowedCidrs">Allowed CIDR notations.</param>
public sealed record VKInternalNetworkRequirement(IReadOnlyList<string> AllowedCidrs) : IVKAuthorizationRequirement
{
    /// <summary>
    /// Gets the default error associated with the requirement failure.
    /// </summary>
    /// <inheritdoc />
    public VKError DefaultError => VKAuthorizationErrors.InternalNetworkDenied;
}
