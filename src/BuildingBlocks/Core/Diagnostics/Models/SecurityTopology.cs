using System.Collections.Generic;

namespace VK.Blocks.Core.Diagnostics;

/// <summary>
/// Represents the unified security topology of a module, including endpoints and system-level metadata.
/// </summary>
public sealed record SecurityTopology
{
    /// <summary>
    /// Gets the name of the module (e.g., "Authentication", "Authorization").
    /// </summary>
    public required string Module { get; init; }

    /// <summary>
    /// Gets the security metadata entries for specific endpoints.
    /// </summary>
    public IReadOnlyCollection<SecurityMetadataEntry> Endpoints { get; init; } = [];

    /// <summary>
    /// Gets any global component catalog information (e.g., Permissions, Auth Schemes, OIDC Providers).
    /// </summary>
    /// <remarks>
    /// The key represents the type of catalog (e.g., "Permissions"), and the value is the catalog data.
    /// </remarks>
    public IReadOnlyDictionary<string, object> Catalogs { get; init; } = new Dictionary<string, object>();
}
