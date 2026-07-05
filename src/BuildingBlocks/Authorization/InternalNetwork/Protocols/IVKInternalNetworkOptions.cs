using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines global static options for Internal Network authorization.
/// </summary>
public interface IVKInternalNetworkOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the list of allowed CIDR ranges for internal network policies.
    /// </summary>
    IReadOnlyList<string> InternalCidrs { get; init; }
}
