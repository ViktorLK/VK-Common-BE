using System.Collections.Generic;

namespace VK.Blocks.Authorization.Common;

/// <summary>
/// Represents descriptive metadata for an authorized endpoint.
/// This model is populated by the source generator to provide a view of the authorization topology.
/// </summary>
public sealed record EndpointAuthorizationInfo
{
    #region Properties

    /// <summary>
    /// Gets the full name of the endpoint (e.g. "Namespace.Controller.Method").
    /// </summary>
    public required string EndpointName { get; init; }

    /// <summary>
    /// Gets the list of permission names required to access this endpoint.
    /// </summary>
    public required string[] Permissions { get; init; }

    /// <summary>
    /// Gets the list of roles allowed to access this endpoint.
    /// </summary>
    public required string[] Roles { get; init; }

    /// <summary>
    /// Gets the minimum rank required, if any.
    /// </summary>
    public string? MinimumRank { get; init; }

    /// <summary>
    /// Gets a value indicating whether the endpoint is restricted to internal networks.
    /// </summary>
    public required bool RequiresInternalNetwork { get; init; }

    /// <summary>
    /// Gets a value indicating whether the endpoint is restricted to working hours.
    /// </summary>
    public required bool RequiresWorkingHours { get; init; }

    /// <summary>
    /// Gets additional custom metadata discovered from attributes.
    /// </summary>
    public IReadOnlyDictionary<string, object?> AdditionalMetadata { get; init; } = new Dictionary<string, object?>();

    #endregion
}
