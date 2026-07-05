using System.Collections.Generic;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines request-level overrides and target parameters for Permissions authorization.
/// </summary>
public interface IVKPermissionOverrides
{
    /// <summary>
    /// Gets the claim type used to extract user permissions, overriding the default setting.
    /// </summary>
    string? PermissionClaimType { get; init; }

    /// <summary>
    /// Gets the collection of permissions required to pass the authorization check.
    /// </summary>
    IEnumerable<string>? Permissions { get; init; }

    /// <summary>
    /// Gets the evaluation mode (All/Any).
    /// </summary>
    VKPermissionEvaluationMode? Mode { get; init; }
}
